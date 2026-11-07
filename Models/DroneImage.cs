
using System.ComponentModel.DataAnnotations;

namespace Agristream.Models;

public class DroneImage
{
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    [Required]
    public byte[] ImageData { get; set; } = Array.Empty<byte>();

    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(450)]
    public string? UploadedByUserId { get; set; }
}
