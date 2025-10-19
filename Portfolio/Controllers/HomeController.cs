using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PortfolioContext _context;

        public HomeController(ILogger<HomeController> logger, PortfolioContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: /
        public async Task<IActionResult> Index()
        {
            // Get main user (assuming single portfolio owner)
            var user = await _context.Users.FirstOrDefaultAsync();

            // Get all services
            var services = await _context.Services
                .Include(s => s.ServiceCategory)
                .Include(s => s.User)
                .OrderByDescending(s => s.ServiceId)
                .ToListAsync();

            // Get all projects
            var projects = await _context.Projects
                .Include(p => p.ProjectCategory)
                .Include(p => p.User)
                .OrderByDescending(p => p.ProjectId)
                .ToListAsync();

            // Get all documents
            var documents = await _context.Documents
                .Include(d => d.User)
                .OrderByDescending(d => d.DocumentId)
                .ToListAsync();

            // Get all news
            var news = await _context.News
                .Include(n => n.User)
                .OrderByDescending(n => n.NewsDate)
                .ToListAsync();

            // Pass data to ViewBag
            ViewBag.User = user;
            ViewBag.Services = services;
            ViewBag.Projects = projects;
            ViewBag.Documents = documents;
            ViewBag.News = news;

            return View();
        }

        public IActionResult Copyright()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
