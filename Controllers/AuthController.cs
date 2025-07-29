using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LoanManagementSystem.Data;
using LoanManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using LoanManagementSystem.Helpers;


namespace LoanManagementSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IEmailHelper _emailHelper;

        public AuthController(ApplicationDbContext context, IEmailHelper emailHelper)
        {
            _context = context;
            _emailHelper = emailHelper;
        }
        // 📝 Get: Login

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewBag.Error = "Invalid credentials.";
                return View();
            }

            // 🧠 Set Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.UserId.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // 🔐 Sign in
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // 💾 Set Session
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRole", user.Role);

            // 🔁 Redirect by role
            return user.Role switch
            {
                "admin" => RedirectToAction("Dashboard", "Admin"),
                "marketing" => RedirectToAction("Dashboard", "Marketing"),
                "calling" => RedirectToAction("Dashboard", "Calling"),
                "office" => RedirectToAction("Dashboard", "Office"),
                _ => RedirectToAction("AccessDenied", "Auth")
            };
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // 🧹 Clear Session and SignOut
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        [Authorize]
        public IActionResult AccessDenied()
        {
            return View("AccessDenied");
        }

        // 📝 Get: Forgot Password 
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                TempData["Message"] = "If the email exists, a reset link has been sent.";
                return View();
            }

            // Generate a token (simple random string for now)
            string token = Guid.NewGuid().ToString();

            // Save token and expiry in database
            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1); // valid for 1 hour
            await _context.SaveChangesAsync();

            // Build reset link
            var resetLink = Url.Action("ResetPassword", "Auth", new { token = token }, Request.Scheme);

            // Send email
            string body = $"Hello {user.FullName},<br/><br/>Click the link below to reset your password:<br/><a href=\"{resetLink}\">{resetLink}</a><br/><br/>This link will expire in 1 hour.";

            await _emailHelper.SendEmailAsync(user.Email, "Reset Your Password", body);

            TempData["Message"] = "If the email exists, a reset link has been sent.";
            return View();
        }
        // 📝 GET: Reset Password
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Invalid reset token.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.UtcNow);
            if (user == null)
            {
                return BadRequest("Invalid or expired token.");
            }

            ViewBag.Token = token;
            return View();
        }

        // 📝 POST: Reset Password
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string token, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ModelState.AddModelError("", "All fields are required.");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                ViewBag.Token = token;
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.UtcNow);
            if (user == null)
            {
                return BadRequest("Invalid or expired token.");
            }

            // Update password and clear token
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            TempData["Message"] = "Password reset successful. You can now log in.";
            return RedirectToAction("Login");
        }

    }
}

