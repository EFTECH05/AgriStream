
using Agristream.Models;
using Microsoft.AspNetCore.Identity;

namespace Agristream.Services
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            string email = configuration["AdminUser:Email"]
                ?? throw new InvalidOperationException(
                    "Admin email is missing.");

            string password = configuration["AdminUser:Password"]
                ?? throw new InvalidOperationException(
                    "Admin password is missing.");

            var admin = await userManager.FindByEmailAsync(email);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(
                    admin, password);

                if (!result.Succeeded)
                {
                    throw new Exception(string.Join("; ",
                        result.Errors.Select(e => e.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                var result = await userManager.AddToRoleAsync(
                    admin, "Admin");

                if (!result.Succeeded)
                {
                    throw new Exception(string.Join("; ",
                        result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
