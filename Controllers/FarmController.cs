
using Agristream.Data;
using Agristream.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Agristream.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FarmController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FarmController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Farm
        // Display all registered farms
        public async Task<IActionResult> Index()
        {
            var farms = await _context.Farms
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(farms);
        }

        // GET: /Farm/Details/1
        // Display farm details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var farm = await _context.Farms
                .FirstOrDefaultAsync(f => f.FarmId == id);

            if (farm == null)
                return NotFound();

            return View(farm);
        }

        // GET: /Farm/Create
        // Display farm registration form
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Farm/Create
        // Save a new farm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("FarmName,Location,SizeInHectares,Description,IsActive")]
            Farm farm)
        {
            if (ModelState.IsValid)
            {
                farm.CreatedAt = DateTime.UtcNow;

                _context.Farms.Add(farm);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Farm registered successfully.";

                return RedirectToAction(nameof(Index));
            }

            return View(farm);
        }

        // GET: /Farm/Edit/1
        // Display farm editing form
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var farm = await _context.Farms.FindAsync(id);

            if (farm == null)
                return NotFound();

            return View(farm);
        }

        // POST: /Farm/Edit/1
        // Update farm information
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("FarmId,FarmName,Location,SizeInHectares,Description,IsActive")]
            Farm farm)
        {
            if (id != farm.FarmId)
                return NotFound();

            if (ModelState.IsValid)
            {
                var existingFarm = await _context.Farms.FindAsync(id);

                if (existingFarm == null)
                    return NotFound();

                existingFarm.FarmName = farm.FarmName;
                existingFarm.Location = farm.Location;
                existingFarm.SizeInHectares = farm.SizeInHectares;
                existingFarm.Description = farm.Description;
                existingFarm.IsActive = farm.IsActive;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Farm information updated successfully.";

                return RedirectToAction(nameof(Index));
            }

            return View(farm);
        }

        // GET: /Farm/Delete/1
        // Display delete confirmation
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var farm = await _context.Farms
                .FirstOrDefaultAsync(f => f.FarmId == id);

            if (farm == null)
                return NotFound();

            return View(farm);
        }

        // POST: /Farm/Delete/1
        // Delete farm from database
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var farm = await _context.Farms.FindAsync(id);

            if (farm == null)
                return NotFound();

            _context.Farms.Remove(farm);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Farm deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
