using Azure;
using Azure.Data.Tables;

namespace Agristream.Functions
{
    public class SensorReadingEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = string.Empty;

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string FarmId { get; set; } = string.Empty;
        public string SensorId { get; set; } = string.Empty;
        public string SensorType { get; set; } = string.Empty;

        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;

        public DateTimeOffset RecordedAt { get; set; }
    }
}