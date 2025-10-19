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
    public class NewsController : Controller
    {
        private readonly PortfolioContext _context;

        public NewsController(PortfolioContext context)
        {
            _context = context;
        }

        // GET: News
        public async Task<IActionResult> Index()
        {
            var news = _context.News.Include(n => n.User).OrderByDescending(n => n.NewsDate);
            return View(await news.ToListAsync());
        }

        // GET: News/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var news = await _context.News
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.NewsId == id);

            return news == null ? NotFound() : View(news);
        }

        // GET: News/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserName");
            return View();
        }

        // POST: News/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(News news, IFormFile? coverImage)
        {
            if (ModelState.IsValid)
            {
                if (coverImage != null && coverImage.Length > 0)
                    news.CoverImageUrl = await UploadFileAsync(coverImage, "news");

                _context.Add(news);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserName", news.UserId);
            return View(news);
        }

        // GET: News/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var news = await _context.News.FindAsync(id);
            if (news == null) return NotFound();

            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserName", news.UserId);
            return View(news);
        }

        // POST: News/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, News news, IFormFile? coverImage)
        {
            if (id != news.NewsId) return NotFound();

            var existingNews = await _context.News.FindAsync(id);
            if (existingNews == null) return NotFound();

            if (ModelState.IsValid)
            {
                existingNews.NewsTitle = news.NewsTitle;
                existingNews.NewsDescription = news.NewsDescription;
                existingNews.NewsDate = news.NewsDate;
                existingNews.UserId = news.UserId;

                if (coverImage != null && coverImage.Length > 0)
                    existingNews.CoverImageUrl = await UploadFileAsync(coverImage, "news");

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.News.Any(e => e.NewsId == id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserName", news.UserId);
            return View(news);
        }

        // GET: News/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var news = await _context.News
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.NewsId == id);

            return news == null ? NotFound() : View(news);
        }

        // POST: News/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news != null)
            {
                _context.News.Remove(news);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: News/AllNews
        public async Task<IActionResult> AllNews(string? searchString)
        {
            var newsQuery = _context.News.Include(n => n.User).OrderByDescending(n => n.NewsDate).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                newsQuery = newsQuery.Where(n =>
                    n.NewsTitle.Contains(searchString) ||
                    n.NewsDescription.Contains(searchString));
            }

            ViewData["SearchString"] = searchString;
            return View(await newsQuery.ToListAsync());
        }

        // Helper method for file upload
        private async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/images/{folder}");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadDir, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/{folder}/{fileName}";
        }
    }
}
