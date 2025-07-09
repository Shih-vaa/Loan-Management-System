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













        // GET: /Notifications
        public async Task<IActionResult> Index()
        {
            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);  // 🔁 Make sure Index.cshtml exists in /Views/Notifications
        }












        // POST: /Notifications/MarkAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }









        // POST: /Notifications/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }











        [HttpGet]
        public async Task<IActionResult> GetLatest()
        {
            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .Select(n => new
                {
                    n.NotificationId,
                    n.Message,
                    n.IsRead,
                    CreatedAt = n.CreatedAt.ToString("dd MMM hh:mm tt")
                })
                .ToListAsync();

            int unreadCount = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            return Json(new
            {
                notifications,
                unreadCount
            });
        }

















        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var note in unread)
            {
                note.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }










    }
}
