using LoanManagementSystem.Data;
using LoanManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Controllers
{
    [Authorize(Roles = "admin")]
    public class CommissionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommissionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var commissions = await _context.Commissions
                .Include(c => c.User)
                .Include(c => c.Lead)
                .ThenInclude(l => l.Customer)
                .ToListAsync();

            return View(commissions);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var commission = await _context.Commissions.FindAsync(id);
            if (commission == null) return NotFound();

            commission.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
