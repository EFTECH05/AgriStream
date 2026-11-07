
using Azure;
using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Agristream.Functions
{
    public class DroneImageAuditFunction
    {
        private readonly TableClient _tableClient;
        private readonly ILogger<DroneImageAuditFunction> _logger;

        public DroneImageAuditFunction(
            IConfiguration configuration,
            ILogger<DroneImageAuditFunction> logger)
        {
            _logger = logger;

            string? tableServiceUri =
                configuration["TableStorage:ServiceUri"];

            if (string.IsNullOrWhiteSpace(tableServiceUri))
            {
                throw new InvalidOperationException(
                    "Table Storage ServiceUri is missing.");
            }

            var tableServiceClient = new TableServiceClient(
                new Uri(tableServiceUri),
                new DefaultAzureCredential());

            _tableClient = tableServiceClient.GetTableClient(
                "DroneImageAudit");
        }

        [Function("DroneImageAudit")]
        public async Task Run(
            [BlobTrigger(
                "drone-images/{name}",
                Connection = "DroneStorage")] Stream blob,
            string name)
        {
            _logger.LogInformation(
                "Drone image uploaded: {FileName}", name);

            try
            {
                await _tableClient.CreateIfNotExistsAsync();

                var auditRecord = new TableEntity(
                    "DroneUploads",
                    Guid.NewGuid().ToString("N"))
                {
                    ["FileName"] = name,
                    ["FileSizeBytes"] = blob.Length,
                    ["Status"] = "Uploaded",
                    ["UploadedAtUtc"] = DateTimeOffset.UtcNow
                };

                await _tableClient.AddEntityAsync(auditRecord);

                _logger.LogInformation(
                    "Drone image {FileName} audit record saved.",
                    name);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to audit drone image {FileName}.",
                    name);

                throw;
            }
        }
    }
}
