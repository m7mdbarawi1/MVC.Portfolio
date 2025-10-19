using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Controllers
{
    public class ProjectCategoriesController : Controller
    {
        private readonly PortfolioContext _context;

        public ProjectCategoriesController(PortfolioContext context)
        {
            _context = context;
        }

        // GET: ProjectCategories
        public async Task<IActionResult> Index()
        {
            return View(await _context.ProjectCategories.ToListAsync());
        }

        // GET: ProjectCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var projectCategory = await _context.ProjectCategories
                .Include(c => c.Projects) // Include related projects if needed
                .FirstOrDefaultAsync(c => c.ProjectCategoryId == id);

            return projectCategory == null ? NotFound() : View(projectCategory);
        }

        // GET: ProjectCategories/Create
        public IActionResult Create() => View();

        // POST: ProjectCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectCategory projectCategory)
        {
            if (!ModelState.IsValid) return View(projectCategory);

            _context.Add(projectCategory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: ProjectCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var projectCategory = await _context.ProjectCategories.FindAsync(id);
            return projectCategory == null ? NotFound() : View(projectCategory);
        }

        // POST: ProjectCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProjectCategory projectCategory)
        {
            if (id != projectCategory.ProjectCategoryId) return NotFound();
            if (!ModelState.IsValid) return View(projectCategory);

            try
            {
                _context.Update(projectCategory);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectCategoryExists(id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ProjectCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var projectCategory = await _context.ProjectCategories
                .Include(c => c.Projects) // Optional: Include projects for info
                .FirstOrDefaultAsync(c => c.ProjectCategoryId == id);

            return projectCategory == null ? NotFound() : View(projectCategory);
        }

        // POST: ProjectCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var projectCategory = await _context.ProjectCategories
                .Include(c => c.Projects) // Include related projects
                .FirstOrDefaultAsync(c => c.ProjectCategoryId == id);

            if (projectCategory != null)
            {
                // Optional: If using SetNull FK rule, update related projects
                foreach (var project in projectCategory.Projects)
                {
                    project.ProjectCategoryId = null;
                }

                _context.ProjectCategories.Remove(projectCategory);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProjectCategoryExists(int id) =>
            _context.ProjectCategories.Any(e => e.ProjectCategoryId == id);
    }
}
