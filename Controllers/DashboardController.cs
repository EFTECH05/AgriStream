
using Microsoft.AspNetCore.Mvc;

namespace Agristream.Controllers
{
    public class DashboardController : Controller
    {
        // Farmer Dashboard
        public IActionResult Index()
        {
            ViewData["Title"] = "Farmer Dashboard";

            return View();
        }
    }
}
