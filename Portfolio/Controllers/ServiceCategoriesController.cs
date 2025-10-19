using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Controllers;

public class ServiceCategoriesController : Controller
{
    private readonly PortfolioContext _context;

    public ServiceCategoriesController(PortfolioContext context) => _context = context;

    // GET: ServiceCategories
    public async Task<IActionResult> Index() =>
        View(await _context.ServiceCategories.ToListAsync());

    // GET: ServiceCategories/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        var serviceCategory = await GetCategoryByIdAsync(id);
        return serviceCategory is null ? NotFound() : View(serviceCategory);
    }

    // GET: ServiceCategories/Create
    public IActionResult Create() => View();

    // POST: ServiceCategories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceCategory serviceCategory)
    {
        if (!ModelState.IsValid) return View(serviceCategory);

        _context.Add(serviceCategory);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: ServiceCategories/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        var serviceCategory = await GetCategoryByIdAsync(id);
        return serviceCategory is null ? NotFound() : View(serviceCategory);
    }

    // POST: ServiceCategories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServiceCategory serviceCategory)
    {
        if (id != serviceCategory.ServiceCategoryId) return NotFound();
        if (!ModelState.IsValid) return View(serviceCategory);

        try
        {
            _context.Update(serviceCategory);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ServiceCategoryExists(id)) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: ServiceCategories/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        var serviceCategory = await GetCategoryByIdAsync(id);
        return serviceCategory is null ? NotFound() : View(serviceCategory);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var serviceCategory = await _context.ServiceCategories
            .FirstOrDefaultAsync(c => c.ServiceCategoryId == id);

        if (serviceCategory is not null)
        {
            _context.ServiceCategories.Remove(serviceCategory);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }


    private bool ServiceCategoryExists(int id) =>
        _context.ServiceCategories.Any(e => e.ServiceCategoryId == id);

    private async Task<ServiceCategory?> GetCategoryByIdAsync(int? id)
    {
        if (id is null) return null;

        return await _context.ServiceCategories
            .Include(c => c.Services) // optional for Delete/Details
            .FirstOrDefaultAsync(c => c.ServiceCategoryId == id);
    }
}
