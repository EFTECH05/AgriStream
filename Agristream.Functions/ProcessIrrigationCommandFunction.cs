

using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text.Json;

namespace Agristream.Functions
{
    public class ProcessIrrigationCommandFunction
    {
        private readonly ILogger<ProcessIrrigationCommandFunction> _logger;
        private readonly DefaultAzureCredential _credential;

        private const string SqlServer =
            "agristream-sql-2026.database.windows.net";

        private const string Database =
            "AgriStreamDB";

        public ProcessIrrigationCommandFunction(
            ILogger<ProcessIrrigationCommandFunction> logger)
        {
            _logger = logger;
            _credential = new DefaultAzureCredential();
        }

        [Function("ProcessIrrigationCommand")]
        public async Task Run(
            [QueueTrigger(
                "irrigation-commands",
                Connection = "QueueStorage")]
            string message)
        {
            try
            {
                _logger.LogInformation(
                    "Processing irrigation command: {Message}",
                    message);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var command =
                    JsonSerializer.Deserialize<IrrigationCommandRequest>(
                        message, options);

                if (command == null ||
                    !int.TryParse(command.FarmId, out int farmId) ||
                    string.IsNullOrWhiteSpace(command.DeviceId) ||
                    string.IsNullOrWhiteSpace(command.Action))
                {
                    throw new InvalidOperationException(
                        "Invalid irrigation command received.");
                }

                string action = command.Action.Trim();

                string operationalStatus =
                    action.ToLowerInvariant() switch
                    {
                        "start" => "Running",
                        "stop" => "Idle",
                        _ => action
                    };

                if (operationalStatus.Length > 50)
                {
                    throw new InvalidOperationException(
                        "Operational status exceeds 50 characters.");
                }

                string lastCommand = command.DurationMinutes > 0
                    ? $"{action} ({command.DurationMinutes} minutes)"
                    : action;

                if (lastCommand.Length > 100)
                {
                    throw new InvalidOperationException(
                        "Last command exceeds 100 characters.");
                }

                _logger.LogInformation(
                    "Requesting Azure SQL access token.");

                var token = await _credential.GetTokenAsync(
                    new TokenRequestContext(
                        new[] { "https://database.windows.net/.default" }));

                _logger.LogInformation(
                    "Azure SQL access token obtained successfully.");

                var connectionString =
                    $"Server=tcp:{SqlServer},1433;" +
                    $"Initial Catalog={Database};" +
                    "Encrypt=True;" +
                    "TrustServerCertificate=False;";

                await using var connection =
                    new SqlConnection(connectionString);

                connection.AccessToken = token.Token;

                _logger.LogInformation(
                    "Opening Azure SQL database connection.");

                await connection.OpenAsync();

                _logger.LogInformation(
                    "Azure SQL database connection opened successfully.");

                const string sql = @"
                    IF EXISTS (
                        SELECT 1
                        FROM DeviceStatuses
                        WHERE FarmId = @FarmId
                          AND DeviceId = @DeviceId
                    )
                    BEGIN
                        UPDATE DeviceStatuses
                        SET OperationalStatus = @OperationalStatus,
                            LastCommand = @LastCommand,
                            LastUpdatedUtc = @LastUpdatedUtc
                        WHERE FarmId = @FarmId
                          AND DeviceId = @DeviceId;
                    END
                    ELSE
                    BEGIN
                        INSERT INTO DeviceStatuses
                        (
                            DeviceId,
                            FarmId,
                            OperationalStatus,
                            LastCommand,
                            LastUpdatedUtc
                        )
                        VALUES
                        (
                            @DeviceId,
                            @FarmId,
                            @OperationalStatus,
                            @LastCommand,
                            @LastUpdatedUtc
                        );
                    END";

                await using var sqlCommand =
                    new SqlCommand(sql, connection);

                sqlCommand.Parameters.Add(
                    "@FarmId", SqlDbType.Int).Value = farmId;

                sqlCommand.Parameters.Add(
                    "@DeviceId", SqlDbType.NVarChar, 100).Value =
                    command.DeviceId.Trim();

                sqlCommand.Parameters.Add(
                    "@OperationalStatus", SqlDbType.NVarChar, 50).Value =
                    operationalStatus;

                sqlCommand.Parameters.Add(
                    "@LastCommand", SqlDbType.NVarChar, 100).Value =
                    lastCommand;

                sqlCommand.Parameters.Add(
                    "@LastUpdatedUtc", SqlDbType.DateTime2).Value =
                    DateTime.UtcNow;

                await sqlCommand.ExecuteNonQueryAsync();

                _logger.LogInformation(
                    "Device {DeviceId} updated successfully. Status: {Status}",
                    command.DeviceId,
                    operationalStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ProcessIrrigationCommand failed. " +
                    "The irrigation command could not be processed.");

                throw;
            }
        }
    }
}
