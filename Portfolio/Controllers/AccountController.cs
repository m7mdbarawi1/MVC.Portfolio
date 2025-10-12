using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Controllers
{
    public class AccountController : Controller
    {
        private readonly PortfolioContext _context;

        public AccountController(PortfolioContext context)
        {
            _context = context;
        }

        // ========================
        // LOGIN
        // ========================
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter username and password.";
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username && u.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var identity = new ClaimsIdentity(claims, "PortfolioAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("PortfolioAuth", principal, new AuthenticationProperties
            {
                IsPersistent = true
            });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // ========================
        // LOGOUT
        // ========================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("PortfolioAuth");
            return RedirectToAction(nameof(Login));
        }

        // ========================
        // REGISTER
        // ========================
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User model)
        {
            if (_context.Users.Any(u => u.UserName == model.UserName))
                ModelState.AddModelError("UserName", "Username already taken.");

            if (_context.Users.Any(u => u.Email == model.Email))
                ModelState.AddModelError("Email", "Email already registered.");

            if (string.IsNullOrEmpty(model.Password) || model.Password.Length < 6)
                ModelState.AddModelError("Password", "Password must be at least 6 characters long.");

            if (!ModelState.IsValid)
                return View(model);

            var user = new User
            {
                UserName = model.UserName,
                Password = model.Password, // Consider hashing here in production
                FirstName = model.FirstName,
                LastName = model.LastName,
                ContactNumber = model.ContactNumber,
                Email = model.Email,
                Description = model.Description ?? ""
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Auto-login after registration
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var identity = new ClaimsIdentity(claims, "PortfolioAuth");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("PortfolioAuth", principal);

            return RedirectToAction("Index", "Home");
        }

        // ========================
        // MY PROFILE (VIEW/UPDATE)
        // ========================
        [Authorize]
        public async Task<IActionResult> MyProfile()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
                return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            return View(user);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MyProfile(User model)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
                return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (_context.Users.Any(u => u.UserName == model.UserName && u.UserId != userId))
                ModelState.AddModelError("UserName", "Username already taken.");

            if (_context.Users.Any(u => u.Email == model.Email && u.UserId != userId))
                ModelState.AddModelError("Email", "Email already registered.");

            if (!ModelState.IsValid)
                return View(model);

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.UserName = model.UserName;
            user.Password = model.Password; // Consider hashing
            user.Email = model.Email;
            user.ContactNumber = model.ContactNumber;
            user.Description = model.Description;

            _context.Update(user);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Profile updated successfully!";
            return View(user);
        }

        // ========================
        // DELETE ACCOUNT
        // ========================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMyAccount()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
                return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            await HttpContext.SignOutAsync("PortfolioAuth");
            return RedirectToAction("Login");
        }
    }
}
