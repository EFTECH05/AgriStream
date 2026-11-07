
using Agristream.Data;
using Agristream.Models;
using Agristream.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Agristream.Controllers;

[Authorize]
public class DroneImagesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DroneImagesController> _logger;

    private const long MaxFileSize = 10 * 1024 * 1024;

    private static readonly string[] AllowedExtensions =
    {
        ".jpg",
        ".jpeg",
        ".png"
    };

    public DroneImagesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<DroneImagesController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    // ==========================================
    // UPLOAD DRONE IMAGE - FARMERS AND ADMINS
    // ==========================================

    [HttpGet]
    [Authorize(Roles = "User,Admin")]
    public IActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "User,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile image)
    {
        if (image == null || image.Length == 0)
        {
            ViewBag.ErrorMessage =
                "Please select an image to upload.";

            return View();
        }

        if (image.Length > MaxFileSize)
        {
            ViewBag.ErrorMessage =
                "The image must not exceed 10 MB.";

            return View();
        }

        string extension = Path.GetExtension(image.FileName)
            .ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            ViewBag.ErrorMessage =
                "Only JPG, JPEG and PNG images are allowed.";

            return View();
        }

        try
        {
            using var memoryStream = new MemoryStream();

            await image.CopyToAsync(memoryStream);

            var droneImage = new DroneImage
            {
                FileName = Path.GetFileName(image.FileName),

                ContentType = extension == ".png"
                    ? "image/png"
                    : "image/jpeg",

                ImageData = memoryStream.ToArray(),

                FileSize = image.Length,

                UploadedAt = DateTime.UtcNow,

                UploadedByUserId =
                    _userManager.GetUserId(User)
            };

            _context.DroneImages.Add(droneImage);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Drone image saved successfully. Image ID: {ImageId}",
                droneImage.Id);

            ViewBag.SuccessMessage =
                "Your drone image was successfully saved to the AgriStream database.";

            ViewBag.ImageId = droneImage.Id;

            return View();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(
                ex,
                "Database error while saving drone image.");

            ViewBag.ErrorMessage =
                "The image could not be saved to the database. Please try again.";

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected drone image upload error.");

            ViewBag.ErrorMessage =
                "An unexpected error occurred while uploading your image.";

            return View();
        }
    }

    // ==========================================
    // ADMIN DRONE IMAGE GALLERY
    // ==========================================

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        var images = await _context.DroneImages
            .AsNoTracking()
            .OrderByDescending(image => image.UploadedAt)
            .Select(image => new DroneImageListItem
            {
                Id = image.Id,
                FileName = image.FileName,
                ContentType = image.ContentType,
                FileSize = image.FileSize,
                UploadedAt = image.UploadedAt
            })
            .ToListAsync();

        return View(images);
    }

    // ==========================================
    // VIEW INDIVIDUAL DRONE IMAGE
    // ==========================================

    [HttpGet]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> ViewImage(int id)
    {
        var image = await _context.DroneImages
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);

        if (image == null)
        {
            return NotFound();
        }

        return File(
            image.ImageData,
            image.ContentType
        );
    }
}
