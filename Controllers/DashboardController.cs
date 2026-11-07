
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agristream.Controllers
{
    [Authorize(Roles = "User")]
    public class DashboardController : Controller
    {
        // GET: Dashboard/Index
        // Only normal users can access the Farmer Dashboard.
        public IActionResult Index()
        {
            ViewData["Title"] = "Farmer Dashboard";

            return View();
        }
    }
}
