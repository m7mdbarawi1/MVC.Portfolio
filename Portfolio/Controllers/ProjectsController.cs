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

namespace Portfolio.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly PortfolioContext _context;

        public ProjectsController(PortfolioContext context)
        {
            _context = context;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var projects = _context.Projects
                .Include(p => p.ProjectCategory)
                .Include(p => p.User);
            return View(await projects.ToListAsync());
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.ProjectCategory)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.ProjectId == id);

            return project == null ? NotFound() : View(project);
        }

        // GET: Projects/Create
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project, IFormFile? coverImage)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdowns(project);
                return View(project);
            }

            if (coverImage != null && coverImage.Length > 0)
            {
                project.CoverImageUrl = await UploadFileAsync(coverImage, "projects");
            }

            _context.Add(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            PopulateDropdowns(project);
            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Project project, IFormFile? coverImage)
        {
            if (id != project.ProjectId) return NotFound();

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(project);
                return View(project);
            }

            var existingProject = await _context.Projects.FindAsync(id);
            if (existingProject == null) return NotFound();

            existingProject.ProjectTitle = project.ProjectTitle;
            existingProject.Description = project.Description;
            existingProject.ProjectCategoryId = project.ProjectCategoryId;
            existingProject.UserId = project.UserId;

            if (coverImage != null && coverImage.Length > 0)
            {
                existingProject.CoverImageUrl = await UploadFileAsync(coverImage, "projects");
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.ProjectCategory)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.ProjectId == id);

            return project == null ? NotFound() : View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/AllProjects
        public async Task<IActionResult> AllProjects()
        {
            var projects = await _context.Projects
                .Include(p => p.ProjectCategory)
                .Include(p => p.User)
                .OrderByDescending(p => p.ProjectId)
                .ToListAsync();
            return View(projects);
        }

        private bool ProjectExists(int id) => _context.Projects.Any(p => p.ProjectId == id);

        // Helper method to populate dropdowns
        private void PopulateDropdowns(Project? project = null)
        {
            ViewData["ProjectCategoryId"] = new SelectList(
                _context.ProjectCategories, "ProjectCategoryId", "CategoryDesc", project?.ProjectCategoryId);
            ViewData["UserId"] = new SelectList(
                _context.Users, "UserId", "UserName", project?.UserId);
        }

        // Helper method to upload files
        private async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/images/{folder}");
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadDir, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/{folder}/{fileName}";
        }
    }
}
