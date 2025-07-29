using LoanManagementSystem.Data;
using LoanManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [ResponseCache(Duration = 30)]
        [HttpGet("GetLatest")]
        public async Task<IActionResult> GetLatest()
        {
            if (!int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value, out int userId))
            {
                return Unauthorized();
            }

            // Fetch unread notifications
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            // Mark them as read
            if (unreadNotifications.Any())
            {
                unreadNotifications.ForEach(n => n.IsRead = true);
                await _context.SaveChangesAsync();
            }

            // Convert CreatedAt to IST
            var indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            // Get latest 5 notifications
            var notificationsList = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync();

            var notifications = notificationsList.Select(n => new
            {
                n.NotificationId,
                n.Message,
                n.IsRead,
                n.CreatedAt,
                Link = n.Link ?? "#"
            });

            return Json(new { notifications, unreadCount = 0 });
        }

        // GET: /Notifications
        public async Task<IActionResult> Index()
        {
            if (!int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value, out int userId))
            {
                return Unauthorized();
            }

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            if (!int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value, out int userId))
                return Unauthorized();

            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId);
            if (notification == null)
                return NotFound();

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            if (!int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value, out int userId))
                return Unauthorized();

            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (unread.Any())
            {
                foreach (var note in unread)
                    note.IsRead = true;

                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "All notifications marked as read." });
        }
    }
}
