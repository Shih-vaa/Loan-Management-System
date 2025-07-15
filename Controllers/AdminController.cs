using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoanManagementSystem.Data;
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

        public async Task<IActionResult> Dashboard()
        {
            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            // Basic metrics
            var metrics = new DashboardMetricsViewModel
            {
                TotalLeads = await _context.Leads.CountAsync(),
                ApprovedLeads = await _context.Leads.CountAsync(l => l.Status == "approved"),
                PendingLeads = await _context.Leads.CountAsync(l => l.Status == "pending"),
                TotalCustomers = await _context.Customers.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                TotalTeams = await _context.Teams.CountAsync(),
                TotalCommissionPaid = await _context.Commissions.Where(c => c.Status == "paid")
                    .SumAsync(c => (decimal?)c.Amount) ?? 0,
                TotalCommissionPending = await _context.Commissions.Where(c => c.Status == "pending")
                    .SumAsync(c => (decimal?)c.Amount) ?? 0,
                TotalCommission = await _context.Commissions.SumAsync(c => (decimal?)c.Amount) ?? 0,
                TotalDocumentsUploaded = await _context.LeadDocuments.CountAsync(),
                PendingDocuments = await _context.LeadDocuments.CountAsync(d => d.Status == "pending"),
                UnassignedLeadsCount = await _context.Leads.CountAsync(l => l.AssignedTo == null),
                LeadStatusCounts = await _context.Leads
                    .GroupBy(l => l.Status ?? "Unknown")
                    .ToDictionaryAsync(g => g.Key, g => g.Count()),
            };

            // Team performance metrics
            var teams = await _context.Teams.Include(t => t.Members).ToListAsync();

            var teamPerformances = new List<TeamPerformanceMetrics>();

            foreach (var team in teams)
            {
                var teamUserIds = team.Members.Select(m => m.UserId).ToList();

                teamPerformances.Add(new TeamPerformanceMetrics
                {
                    TeamName = team.TeamName,
                    TotalLeadsGenerated = await _context.Leads.CountAsync(l => teamUserIds.Contains(l.LeadGeneratorId)),
                    LeadsAssigned = await _context.Leads.CountAsync(l => l.AssignedTo != null && teamUserIds.Contains((int)l.AssignedTo)),
                    DocumentsUploaded = await _context.LeadDocuments.CountAsync(d => d.UploadedBy.HasValue && teamUserIds.Contains(d.UploadedBy.Value)),
                    DocumentsVerified = await _context.LeadDocuments.CountAsync(d => d.VerifiedBy.HasValue && teamUserIds.Contains(d.VerifiedBy.Value)),
                    LeadsApproved = await _context.Leads.CountAsync(l => l.Status == "approved" && l.AssignedTo != null && teamUserIds.Contains((int)l.AssignedTo)),
                    TotalCommission = await _context.Commissions
                        .Where(c => teamUserIds.Contains(c.UserId))
                        .SumAsync(c => (decimal?)c.Amount) ?? 0
                });
            }

            metrics.TeamPerformances = teamPerformances;

            return View(metrics);
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult ExportMetrics()
        {
            var teams = _context.Teams
                .Include(t => t.Members)
                    .ThenInclude(m => m.User)
                .ToList();

            var leads = _context.Leads.ToList();
            var docs = _context.LeadDocuments.ToList();
            var commissions = _context.Commissions.ToList();

            var csv = "Team,Leads Generated,Leads Assigned,Docs Uploaded,Docs Verified,Leads Approved,Total Commission\n";

            foreach (var team in teams)
            {
                var memberIds = team.Members.Select(m => m.UserId).ToList();

                var generated = leads.Count(l => memberIds.Contains(l.LeadGeneratorId));
                var assigned = leads.Count(l => l.AssignedTo.HasValue && memberIds.Contains(l.AssignedTo.Value));

                var uploaded = docs.Count(d => d.UploadedBy != null && memberIds.Contains(d.UploadedBy.Value));
                var verified = docs.Count(d => d.VerifiedBy != null && memberIds.Contains(d.VerifiedBy.Value));
                var approved = leads.Count(l => l.Status == "approved" && l.AssignedTo != null && memberIds.Contains(l.AssignedTo.Value));
                var totalCommission = commissions
                    .Where(c => c.UserId != null && memberIds.Contains(c.UserId))
                    .Sum(c => c.Amount);

                csv += $"{team.TeamName},{generated},{assigned},{uploaded},{verified},{approved},{totalCommission}\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", "TeamPerformanceMetrics.csv");
        }







[Authorize(Roles = "admin")]
public async Task<IActionResult> TeamDrilldown(string teamName)
{
    if (string.IsNullOrEmpty(teamName))
        return BadRequest("Team name is required.");

    var team = await _context.Teams
        .Include(t => t.Members)
            .ThenInclude(m => m.User)
        .FirstOrDefaultAsync(t => t.TeamName == teamName);

    if (team == null) return NotFound("Team not found.");

    var memberIds = team.Members.Select(m => m.UserId).ToList();

    var leadsGenerated = await _context.Leads.CountAsync(l => memberIds.Contains(l.LeadGeneratorId));
    var leadsAssigned = await _context.Leads.CountAsync(l => l.AssignedTo != null && memberIds.Contains(l.AssignedTo.Value));
    var docsUploaded = await _context.LeadDocuments.CountAsync(d => d.UploadedBy != null && memberIds.Contains(d.UploadedBy.Value));
    var docsVerified = await _context.LeadDocuments.CountAsync(d => d.VerifiedBy != null && memberIds.Contains(d.VerifiedBy.Value));
    var leadsApproved = await _context.Leads.CountAsync(l => l.Status == "approved" && l.AssignedTo != null && memberIds.Contains(l.AssignedTo.Value));
    var commission = await _context.Commissions
        .Where(c => memberIds.Contains(c.UserId))
        .SumAsync(c => (decimal?)c.Amount) ?? 0;

    ViewBag.TeamName = team.TeamName;
    ViewBag.Members = team.Members.Select(m => m.User?.FullName).ToList();

    var vm = new TeamPerformanceMetrics
    {
        TeamName = team.TeamName,
        TotalLeadsGenerated = leadsGenerated,
        LeadsAssigned = leadsAssigned,
        DocumentsUploaded = docsUploaded,
        DocumentsVerified = docsVerified,
        LeadsApproved = leadsApproved,
        TotalCommission = commission
    };

    return View("TeamDrilldown", vm); // 👈 We'll create this view next
}

    }
}
