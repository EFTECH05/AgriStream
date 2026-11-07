
using Agristream.Data;
using Agristream.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Agristream.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Admin/Index
        // Administrator dashboard with live database statistics.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Admin Dashboard";

            // Load each statistic independently so that
            // one failed query does not hide the others.

            try
            {
                ViewBag.TotalUsers =
                    await _userManager.Users.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user count.");
                ViewBag.TotalUsers = "--";
            }

            try
            {
                ViewBag.TotalFarms =
                    await _context.Farms.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve farm count.");
                ViewBag.TotalFarms = "--";
            }

            try
            {
                ViewBag.TotalDroneImages =
                    await _context.DroneImages.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve drone image count.");
                ViewBag.TotalDroneImages = "--";
            }

            // Azure Table Storage telemetry integration is pending.
            ViewBag.TotalSensorReadings = "--";

            return View();
        }

        // GET: Admin/Users
        // Display registered user accounts.
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            try
            {
                var users = await _userManager.Users
                    .AsNoTracking()
                    .OrderBy(user => user.Email)
                    .Select(user => new AdminUserViewModel
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email ?? "",
                        UserName = user.UserName ?? ""
                    })
                    .ToListAsync();

                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve registered users.");

                TempData["ErrorMessage"] =
                    "Unable to load registered users. Please try again.";

                return RedirectToAction(nameof(Index));
            }
        }
    }

    // View model used by the administrator user listing.
    public class AdminUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;
    }
}

