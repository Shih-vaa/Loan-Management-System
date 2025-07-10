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
        if (!int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value, out int userId))
        {
            return Unauthorized();
        }

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

    // POST: /Notifications/MarkAllAsRead
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead()
    {
        if (!int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value, out int userId))
        {
            return Unauthorized();
        }

        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var note in unread)
        {
            note.IsRead = true;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
}
