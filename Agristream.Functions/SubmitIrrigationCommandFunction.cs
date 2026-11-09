
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Agristream.Functions
{
    public class SubmitIrrigationCommandFunction
    {
        private readonly QueueClient _queueClient;
        private readonly ILogger<SubmitIrrigationCommandFunction> _logger;

        public SubmitIrrigationCommandFunction(
            IConfiguration configuration,
            ILogger<SubmitIrrigationCommandFunction> logger)
        {
            _logger = logger;

            string? connectionString =
                configuration["QueueStorage"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Queue Storage connection string is missing.");
            }

            _queueClient = new QueueClient(
                connectionString,
                "irrigation-commands");
        }

        [Function("SubmitIrrigationCommand")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "post",
                Route = "irrigation/commands")]
            HttpRequestData request)
        {
            var command =
                await request.ReadFromJsonAsync<IrrigationCommandRequest>();

            if (command == null ||
                string.IsNullOrWhiteSpace(command.FarmId) ||
                string.IsNullOrWhiteSpace(command.DeviceId) ||
                string.IsNullOrWhiteSpace(command.Action))
            {
                var badRequest =
                    request.CreateResponse(HttpStatusCode.BadRequest);

                await badRequest.WriteStringAsync(
                    "FarmId, DeviceId and Action are required.");

                return badRequest;
            }

            await _queueClient.CreateIfNotExistsAsync();

            string message = JsonSerializer.Serialize(command);

            await _queueClient.SendMessageAsync(message);

            _logger.LogInformation(
                "Irrigation command queued for device {DeviceId}.",
                command.DeviceId);

            var response =
                request.CreateResponse(HttpStatusCode.Accepted);

            await response.WriteAsJsonAsync(new
            {
                Message = "Irrigation command successfully queued.",
                Command = command
            });

            return response;
        }
    }

    public class IrrigationCommandRequest
    {
        public string FarmId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
    }
}
