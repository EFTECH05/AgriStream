
using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Agristream.Functions
{
    public class GetSensorReadingsFunction
    {
        private readonly TableClient _tableClient;
        private readonly ILogger<GetSensorReadingsFunction> _logger;

        public GetSensorReadingsFunction(
            IConfiguration configuration,
            ILogger<GetSensorReadingsFunction> logger)
        {
            _logger = logger;

            string? serviceUri =
                configuration["TableStorage:ServiceUri"];

            if (string.IsNullOrWhiteSpace(serviceUri))
            {
                throw new InvalidOperationException(
                    "Table Storage ServiceUri is missing.");
            }

            var tableServiceClient = new TableServiceClient(
                new Uri(serviceUri),
                new DefaultAzureCredential());

            _tableClient = tableServiceClient.GetTableClient(
                "SensorReadings");
        }

        [Function("GetSensorReadings")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "get",
                Route = "farms/{farmId}/readings")]
            HttpRequestData request,
            string farmId)
        {
            _logger.LogInformation(
                "Retrieving sensor readings for farm {FarmId}",
                farmId);

            var readings = new List<SensorReadingEntity>();

            await foreach (var reading in
                _tableClient.QueryAsync<SensorReadingEntity>(
                    filter: entity => entity.PartitionKey == farmId))
            {
                readings.Add(reading);
            }

            var latestReadings = readings
                .OrderByDescending(x => x.RecordedAt)
                .Take(50)
                .ToList();

            var response = request.CreateResponse(HttpStatusCode.OK);

            await response.WriteAsJsonAsync(new
            {
                FarmId = farmId,
                Count = latestReadings.Count,
                Readings = latestReadings
            });

            return response;
        }
    }
}
