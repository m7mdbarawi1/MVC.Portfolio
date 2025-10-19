using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data;
using Portfolio.Models;
using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;

namespace Portfolio.Controllers
{
    public class AccountController : Controller
    {
        private readonly PortfolioContext _context;

        public AccountController(PortfolioContext context)
        {
            _context = context;
        }

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
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter username and password.";
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username && u.Password == password);
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

            return RedirectToAction("Admin", "Dashboard");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("PortfolioAuth");
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        public IActionResult Register() => View();

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
                Password = model.Password,
                FirstName = model.FirstName,
                LastName = model.LastName,
                ContactNumber = model.ContactNumber,
                Email = model.Email,
                Description = model.Description ?? ""
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await SignInUser(user);

            return RedirectToAction("Admin", "Dashboard");
        }

        [Authorize]
        public async Task<IActionResult> MyProfile()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login");
            return View(user);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MyProfile(User model, IFormFile? coverImage)
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login");

            if (_context.Users.Any(u => u.UserName == model.UserName && u.UserId != user.UserId))
                ModelState.AddModelError("UserName", "Username already taken.");
            if (_context.Users.Any(u => u.Email == model.Email && u.UserId != user.UserId))
                ModelState.AddModelError("Email", "Email already registered.");

            if (!ModelState.IsValid) return View(model);

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.UserName = model.UserName;
            user.Password = model.Password;
            user.Email = model.Email;
            user.ContactNumber = model.ContactNumber;
            user.Description = model.Description;

            // Handle profile image upload
            if (coverImage != null && coverImage.Length > 0)
            {
                user.CoverImageUrl = await UploadFileAsync(coverImage, "profiles");
            }

            _context.Update(user);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Profile updated successfully!";
            return View(user);
        }

        // Helper for file upload
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



        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMyAccount()
        {
            var user = await GetCurrentUser();
            if (user == null) return RedirectToAction("Login");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            await HttpContext.SignOutAsync("PortfolioAuth");
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult HomeRedirect()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Admin", "Dashboard");
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public IActionResult ForgotPassword() 
        {
            return View(); 
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Please enter your email.";
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Message = "Uncorrect email or password";
                return View();
            }

            try
            {
                SendCurrentPasswordEmail(user.Password, user.Email);
                ViewBag.Message = $"Your current password has been sent to {user.Email}.";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Unable to send email: " + ex.Message;
            }

            return View();
        }

        private void SendCurrentPasswordEmail(string password, string toEmail)
        {
            var fromEmail = "m7mdbarawi@gmail.com";
            var appPassword = "mzodokxxxbgpwkxg";
            using var smtpClient = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(fromEmail, appPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, "Portfolio Support"),
                Subject = "Your Portfolio Account Password",
                Body = $"<p>Hello,</p><p>Your current password is: <strong>{password}</strong></p><p>For security, consider changing it after logging in.</p>",
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);
            try
            {
                smtpClient.Send(mailMessage);
                Console.WriteLine($"Email sent successfully to {toEmail}"); // For debugging
            }
            catch (SmtpException smtpEx)
            {
                // This will give you specific SMTP error codes and messages
                Console.WriteLine($"SMTP Error sending email to {toEmail}: Status Code: {smtpEx.StatusCode}, Message: {smtpEx.Message}");
                throw; // Re-throw to be caught by the outer catch block in ForgotPassword action
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Generic Error sending email to {toEmail}: {ex.Message}");
                throw;
            }
        }

        private async Task<User?> GetCurrentUser()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId)) return null;
            return await _context.Users.FindAsync(userId);
        }

        private async Task SignInUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var identity = new ClaimsIdentity(claims, "PortfolioAuth");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("PortfolioAuth", principal);
        }
    }
}
