
using Microsoft.AspNetCore.Identity;

namespace Agristream.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
