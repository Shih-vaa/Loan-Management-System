using LoanManagementSystem.Data;
using LoanManagementSystem.Helpers;
using LoanManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Controllers
{
   [Authorize]
public class CommissionController : Controller
{
    private readonly ApplicationDbContext _context;

    public CommissionController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? status = null)
    {
        string role = HttpContext.Session.GetString("UserRole") ?? "";
        int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

        var commissionsQuery = _context.Commissions
            .Include(c => c.User)
            .Include(c => c.Lead)
            .ThenInclude(l => l.Customer)
            .AsQueryable();

        if (role == "calling")
        {
            commissionsQuery = commissionsQuery.Where(c => c.UserId == userId);
        }

        if (!string.IsNullOrEmpty(status))
        {
            commissionsQuery = commissionsQuery.Where(c => c.Status == status);
        }

        var commissions = await commissionsQuery.ToListAsync();

        // Totals
        var totals = await _context.Commissions
            .GroupBy(c => c.Status)
            .Select(g => new
            {
                Status = g.Key,
                Amount = g.Sum(c => c.Amount)
            }).ToDictionaryAsync(g => g.Status, g => g.Amount);

        ViewBag.TotalPending = totals.ContainsKey("pending") ? totals["pending"] : 0;
        ViewBag.TotalApproved = totals.ContainsKey("approved") ? totals["approved"] : 0;
        ViewBag.TotalPaid = totals.ContainsKey("paid") ? totals["paid"] : 0;
        ViewBag.SelectedStatus = status;

        return View(commissions);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var commission = await _context.Commissions
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.CommissionId == id);

        if (commission == null) return NotFound();

        commission.Status = status;
        await _context.SaveChangesAsync();

        // ✅ Notify the user
        await NotificationHelper.AddNotificationAsync(_context,
            commission.UserId,
            $"Your commission for LMS-{commission.LeadId:D4} is now marked as '{status.ToUpper()}'");

        return RedirectToAction(nameof(Index));
    }
}

}
