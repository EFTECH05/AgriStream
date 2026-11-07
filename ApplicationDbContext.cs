
using Agristream.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Agristream.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Farm management table
        public DbSet<Farm> Farms { get; set; }

        // Drone image storage table
        public DbSet<DroneImage> DroneImages { get; set; }

        // Irrigation device status table
        public DbSet<DeviceStatus> DeviceStatuses { get; set; }
    }
}