using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LoanManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using LoanManagementSystem.Models.ViewModels;
namespace LoanManagementSystem.Controllers

{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 👈 Don't forget this!

        public async Task<IActionResult> Dashboard()
        {
            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            // 🧑‍🤝‍🧑 Fetch team memberships and teammates
            var teamMemberships = _context.TeamMembers
                .Include(tm => tm.Team)
                .Include(tm => tm.User)
                .Where(tm => tm.UserId == userId)
                .ToList();

            var teamIds = teamMemberships.Select(tm => tm.TeamId).ToList();

            var teammates = _context.TeamMembers
                .Include(tm => tm.User)
                .Include(tm => tm.Team)
                .Where(tm => teamIds.Contains(tm.TeamId))
                .ToList();

            ViewBag.Name = HttpContext.Session.GetString("UserName");
            ViewBag.TeamMemberships = teamMemberships.Select(tm => tm.Team).Distinct().ToList();
            ViewBag.Teammates = teammates;

            // 📊 Fetch metrics
            var metrics = new DashboardMetricsViewModel
            {
                TotalLeads = await _context.Leads.CountAsync(),
                ApprovedLeads = await _context.Leads.CountAsync(l => l.Status == "approved"),
                PendingLeads = await _context.Leads.CountAsync(l => l.Status == "pending"),

                TotalCustomers = await _context.Customers.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                TotalTeams = await _context.Teams.CountAsync(),
                TotalCommissionPaid = await _context.Commissions
          .Where(c => c.Status == "paid").SumAsync(c => (decimal?)c.Amount) ?? 0,
                TotalCommissionPending = await _context.Commissions
          .Where(c => c.Status == "pending").SumAsync(c => (decimal?)c.Amount) ?? 0,

                // ✅ Add this:
                TotalCommission = await _context.Commissions.SumAsync(c => (decimal?)c.Amount) ?? 0,

                TotalDocumentsUploaded = await _context.LeadDocuments.CountAsync(),
                PendingDocuments = await _context.LeadDocuments.CountAsync(d => d.Status == "pending"),
                UnassignedLeadsCount = await _context.Leads
    .CountAsync(l => l.AssignedTo == null),

                LeadStatusCounts = await _context.Leads
          .GroupBy(l => l.Status)
          .ToDictionaryAsync(g => g.Key ?? "Unknown", g => g.Count())


            };



            return View(metrics); // 👈 Send to view
        }


    }
}
