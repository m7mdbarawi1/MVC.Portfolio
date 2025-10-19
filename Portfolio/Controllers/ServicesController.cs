using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Controllers;

public class ServicesController : Controller
{
    private readonly PortfolioContext _context;

    public ServicesController(PortfolioContext context) => _context = context;

    // GET: Services
    public async Task<IActionResult> Index()
    {
        var services = _context.Services
            .Include(s => s.ServiceCategory)
            .Include(s => s.User);
        return View(await services.ToListAsync());
    }

    // GET: Services/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var service = await _context.Services
            .Include(s => s.ServiceCategory)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.ServiceId == id);

        return service is null ? NotFound() : View(service);
    }

    // GET: Services/Create
    public IActionResult Create()
    {
        PopulateDropdowns();
        return View();
    }

    // POST: Services/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Service service, IFormFile? coverImage)
    {
        if (!ModelState.IsValid)
        {
            PopulateDropdowns(service);
            return View(service);
        }

        if (coverImage is { Length: > 0 })
            service.CoverImageUrl = await UploadFileAsync(coverImage, "services");

        _context.Add(service);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Services/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var service = await _context.Services.FindAsync(id);
        if (service is null) return NotFound();

        PopulateDropdowns(service);
        return View(service);
    }

    // POST: Services/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Service service, IFormFile? coverImage)
    {
        if (id != service.ServiceId) return NotFound();
        if (!ModelState.IsValid)
        {
            PopulateDropdowns(service);
            return View(service);
        }

        var existingService = await _context.Services.FindAsync(id);
        if (existingService is null) return NotFound();

        existingService.ServiceTitle = service.ServiceTitle;
        existingService.Description = service.Description;
        existingService.ServiceCategoryId = service.ServiceCategoryId;
        existingService.UserId = service.UserId;

        if (coverImage is { Length: > 0 })
            existingService.CoverImageUrl = await UploadFileAsync(coverImage, "services");

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ServiceExists(id)) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Services/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var service = await _context.Services
            .Include(s => s.ServiceCategory)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.ServiceId == id);

        return service is null ? NotFound() : View(service);
    }

    // POST: Services/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service != null)
        {
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Services/AllServices
    public async Task<IActionResult> AllServices(string? searchTerm)
    {
        var query = _context.Services
            .Include(s => s.ServiceCategory)
            .Include(s => s.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(s =>
                s.ServiceTitle.Contains(searchTerm) ||
                s.Description.Contains(searchTerm) ||
                s.ServiceCategory.CategoryDesc.Contains(searchTerm) ||
                s.User.UserName.Contains(searchTerm));
        }

        var services = await query
            .OrderByDescending(s => s.ServiceId)
            .ToListAsync();

        return View("AllServices", services);
    }

    private bool ServiceExists(int id) => _context.Services.Any(s => s.ServiceId == id);

    // Helper: Populate dropdowns
    private void PopulateDropdowns(Service? service = null)
    {
        ViewData["ServiceCategoryId"] = new SelectList(
            _context.ServiceCategories, "ServiceCategoryId", "CategoryDesc", service?.ServiceCategoryId);

        ViewData["UserId"] = new SelectList(
            _context.Users, "UserId", "UserName", service?.UserId);
    }

    // Helper: Upload files (static because it does not use instance data)
    private static async Task<string> UploadFileAsync(IFormFile file, string folder)
    {
        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/images/{folder}");
        if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(uploadDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/images/{folder}/{fileName}";
    }
}
