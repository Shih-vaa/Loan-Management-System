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

            // 🔐 Generate reset token (for secure reset link)
            string resetToken = Guid.NewGuid().ToString();
            user.ResetToken = BCrypt.Net.BCrypt.HashPassword(resetToken); // hash for security
            user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);

            // 🔢 Generate OTP
            string otp = new Random().Next(100000, 999999).ToString();
            user.OtpCode = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(15);

            await _context.SaveChangesAsync();

            // 📬 Send email with OTP
            string body = $@"
        Hello {user.FullName},<br/><br/>
        You requested a password reset.<br/>
        Please use the following OTP to continue:<br/>
        <h3>{otp}</h3>
        This OTP will expire in 15 minutes.<br/><br/>
        If you did not request this, please ignore this email.";

            await _emailHelper.SendEmailAsync(user.Email, "Your OTP for Password Reset", body);

            TempData["Email"] = email;
            TempData["Message"] = "An OTP has been sent to your email.";
            return RedirectToAction("VerifyOtp");
        }





        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyOtp()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp(string email, string otp)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || user.OtpCode != otp || user.OtpExpiry < DateTime.UtcNow)
            {
                TempData["Message"] = "Invalid or expired OTP.";
                TempData["Email"] = email;
                return RedirectToAction("VerifyOtp");
            }

            // ✅ OTP is correct
            TempData["ResetEmail"] = user.Email;
            TempData["TokenVerified"] = true;
            return RedirectToAction("ResetPassword");
        }


        // 📝 GET: Reset Password
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword()
        {
            if (TempData["TokenVerified"] == null || TempData["ResetEmail"] == null)
            {
                return RedirectToAction("ForgotPassword");
            }

            TempData.Keep("ResetEmail"); // Preserve for POST
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string email, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Passwords do not match.";
                TempData["ResetEmail"] = email;

            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || user.OtpCode == null || user.OtpExpiry < DateTime.UtcNow)
            {
                TempData["Error"] = "Invalid or expired reset attempt.";
                return RedirectToAction("ForgotPassword");
            }

            // ✅ Update password and clear reset info
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;
            user.OtpCode = null;
            user.OtpExpiry = null;

            await _context.SaveChangesAsync();

            // ✅ Send confirmation email
            string subject = "Your Password Has Been Changed";
            string body = $@"
        Hello {user.FullName},<br/><br/>
        Your password for LMS account was successfully updated on <strong>{DateTime.Now.ToString("f")}</strong>.<br/>
        If this wasn't you, please contact support immediately.<br/><br/>
        Regards,<br/>
        Loan Management System";

            await _emailHelper.SendEmailAsync(user.Email, subject, body);

            TempData["Message"] = "Password has been reset. Please login.";
            return RedirectToAction("Login");
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResendOtp([FromBody] EmailRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            // Generate new OTP
            var otp = new Random().Next(100000, 999999).ToString();
            user.OtpCode = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(15);
            await _context.SaveChangesAsync();

            // Send email
            string body = $"Your new OTP is: <strong>{otp}</strong>. It is valid for 15 minutes.";
            await _emailHelper.SendEmailAsync(user.Email, "Resent OTP for Password Reset", body);

            return Ok();
        }

        public class EmailRequest
        {
            public string Email { get; set; }
        }



    }
}

