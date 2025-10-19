using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Controllers;

public class UsersController : Controller
{
    private readonly PortfolioContext _context;

    public UsersController(PortfolioContext context) => _context = context;

    // GET: Users
    public async Task<IActionResult> Index()
        => View(await _context.Users.ToListAsync());

    // GET: Users/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == id);

        return user is null ? NotFound() : View(user);
    }

    // GET: Users/Create
    public IActionResult Create() => View();

    // POST: Users/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(User user)
    {
        if (!ModelState.IsValid) return View(user);

        _context.Add(user);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Users/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var user = await _context.Users.FindAsync(id);
        return user is null ? NotFound() : View(user);
    }

    // POST: Users/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, User user)
    {
        if (id != user.UserId) return NotFound();
        if (!ModelState.IsValid) return View(user);

        try
        {
            _context.Update(user);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExists(user.UserId)) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Users/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == id);

        return user is null ? NotFound() : View(user);
    }

    // POST: Users/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool UserExists(int id)
        => _context.Users.Any(u => u.UserId == id);
}
