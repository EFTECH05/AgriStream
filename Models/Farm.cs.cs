
using System.ComponentModel.DataAnnotations;

namespace Agristream.Models
{
    public class Farm
    {
        public int FarmId { get; set; }

        [Required(ErrorMessage = "Farm name is required.")]
        [StringLength(100)]
        [Display(Name = "Farm Name")]
        public string FarmName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Farm location is required.")]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Farm size is required.")]
        [Range(0.1, 100000)]
        [Display(Name = "Farm Size (Hectares)")]
        public double SizeInHectares { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Date Registered")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
