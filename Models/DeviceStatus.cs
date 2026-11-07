using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agristream.Models
{
    public class DeviceStatus
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string DeviceId { get; set; } = string.Empty;

        public int FarmId { get; set; }

        [ForeignKey(nameof(FarmId))]
        public Farm? Farm { get; set; }

        [Required]
        [MaxLength(50)]
        public string OperationalStatus { get; set; } = "Idle";

        [MaxLength(100)]
        public string? LastCommand { get; set; }

        public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
    }
}