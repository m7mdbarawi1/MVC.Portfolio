using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;
using Microsoft.AspNetCore.StaticFiles;

namespace Portfolio.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly PortfolioContext _context;
        private readonly IWebHostEnvironment _env;

        public DocumentsController(PortfolioContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Documents
        public async Task<IActionResult> Index()
        {
            var documents = _context.Documents.Include(d => d.User).OrderByDescending(d => d.DocumentId);
            return View(await documents.ToListAsync());
        }

        // GET: Documents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var document = await _context.Documents
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DocumentId == id);

            return document == null ? NotFound() : View(document);
        }

        // GET: Documents/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserName");
            return View();
        }

        // POST: Documents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Document document, IFormFile? coverImageFile, IFormFile? documentFile)
        {
            if (!ModelState.IsValid)
            {
                ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserName", document.UserId);
                return View(document);
            }

            if (coverImageFile != null) document.CoverImageUrl = await UploadFileAsync(coverImageFile, "images");
            if (documentFile != null) document.DocumentUrl = await UploadFileAsync(documentFile, "documents");

            _context.Add(document);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Documents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var document = await _context.Documents.FindAsync(id);
            if (document == null) return NotFound();

            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserName", document.UserId);
            return View(document);
        }

        // POST: Documents/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Document document, IFormFile? coverImageFile, IFormFile? documentFile)
        {
            if (id != document.DocumentId) return NotFound();
            if (!ModelState.IsValid)
            {
                ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserName", document.UserId);
                return View(document);
            }

            var existingDocument = await _context.Documents.FindAsync(id);
            if (existingDocument == null) return NotFound();

            existingDocument.DocumentTitle = document.DocumentTitle;
            existingDocument.UserId = document.UserId;

            if (coverImageFile != null) existingDocument.CoverImageUrl = await UploadFileAsync(coverImageFile, "images");
            if (documentFile != null) existingDocument.DocumentUrl = await UploadFileAsync(documentFile, "documents");

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocumentExists(id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Documents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var document = await _context.Documents
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DocumentId == id);

            return document == null ? NotFound() : View(document);
        }

        // POST: Documents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document != null)
            {
                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Documents/AllDocuments
        public async Task<IActionResult> AllDocuments(string? searchString)
        {
            var documentsQuery = _context.Documents
                .Include(d => d.User)
                .OrderByDescending(d => d.DocumentId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                documentsQuery = documentsQuery.Where(d =>
                    d.DocumentTitle.Contains(searchString) ||
                    d.User.UserName.Contains(searchString));
            }

            ViewData["SearchString"] = searchString;
            return View(await documentsQuery.ToListAsync());
        }

        // GET: Documents/Download/5
        public async Task<IActionResult> Download(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null || string.IsNullOrEmpty(document.DocumentUrl)) return NotFound();

            var filePath = Path.Combine(_env.WebRootPath, document.DocumentUrl.TrimStart('/'));
            if (!System.IO.File.Exists(filePath)) return NotFound();

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType)) contentType = "application/octet-stream";

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return File(stream, contentType, Path.GetFileName(filePath));
        }

        private bool DocumentExists(int id) => _context.Documents.Any(e => e.DocumentId == id);

        private async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            var uploadPath = Path.Combine(_env.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadPath);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var fileFullPath = Path.Combine(uploadPath, fileName);

            await using var stream = new FileStream(fileFullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/{folder}/{fileName}";
        }
    }
}
