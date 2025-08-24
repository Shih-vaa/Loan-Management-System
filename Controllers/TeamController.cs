using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoanManagementSystem.Data;
using LoanManagementSystem.Models;

namespace LoanManagementSystem.Controllers
{
    [Authorize(Roles = "admin")]
    public class TeamController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeamController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Team
        public async Task<IActionResult> Index()
        {
            var teams = await _context.Teams
                .Include(t => t.Creator)
                .Include(t => t.Members)
                .ThenInclude(m => m.User)
                .ToListAsync();

            return View(teams);
        }

        // GET: /Team/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Team/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Team team)
        {
            if (!ModelState.IsValid) return View(team);

            team.CreatedBy = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Team/Members/{id}
        public async Task<IActionResult> Members(int id)
        {
            var team = await _context.Teams
                .Include(t => t.Members!)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(t => t.TeamId == id);

            if (team == null) return NotFound();

            var currentUserIds = team.Members.Select(m => m.UserId).ToList();
            var availableUsers = await _context.Users
                .Where(u => !currentUserIds.Contains(u.UserId))
                .ToListAsync();

            ViewBag.AllUsers = availableUsers;
            return View(team);
        }

        // POST: /Team/AddMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(int teamId, int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Members", new { id = teamId });
            }

            bool alreadyMember = await _context.TeamMembers
                .AnyAsync(tm => tm.TeamId == teamId && tm.UserId == userId);

            if (alreadyMember)
            {
                TempData["Error"] = "User is already a member of this team.";
                return RedirectToAction("Members", new { id = teamId });
            }

            var newMember = new TeamMember
            {
                TeamId = teamId,
                UserId = userId
            };

            _context.TeamMembers.Add(newMember);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Member added successfully.";
            return RedirectToAction("Members", new { id = teamId });
        }

        // POST: /Team/RemoveMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int teamId, int userId)
        {
            var member = await _context.TeamMembers.FindAsync(teamId, userId);
            if (member != null)
            {
                _context.TeamMembers.Remove(member);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Members", new { id = teamId });
        }

        // DELETE TEAM
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null) return NotFound();

            if (await _context.TeamMembers.AnyAsync(m => m.TeamId == id))
            {
                TempData["Error"] = "Cannot delete a team that has members assigned.";
                return RedirectToAction("Index");
            }

            var hasLeads = await _context.Leads.AnyAsync(l => l.AssignedTo != null &&
                _context.TeamMembers.Any(tm => tm.TeamId == id && tm.UserId == l.AssignedTo));

            if (hasLeads)
            {
                TempData["Error"] = "Cannot delete a team with active leads.";
                return RedirectToAction("Index");
            }

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Team deleted successfully.";
            return RedirectToAction("Index");
        }

        // VIEW TEAM DETAILS
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> View(int id)
        {
            var team = await _context.Teams
                .Include(t => t.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(t => t.TeamId == id);

            if (team == null)
                return NotFound();

            // Collect stats for each member
            var memberStats = new List<MemberLeadStatsViewModel>();

            foreach (var member in team.Members)
            {
                var userId = member.UserId;

                memberStats.Add(new MemberLeadStatsViewModel
                {
                    UserId = userId,
                    FullName = member.User.FullName,
                    Role = member.User.Role,
                    LeadsGenerated = await _context.Leads.CountAsync(l => l.LeadGeneratorId == userId),
                    LeadsAssigned = await _context.Leads.CountAsync(l => l.AssignedTo == userId),
                    LeadsApproved = await _context.Leads.CountAsync(l => l.AssignedTo == userId && l.Status == "approved"),
                    DocumentsUploaded = await _context.LeadDocuments.CountAsync(d => d.UploadedBy == userId),
                    DocumentsVerified = await _context.LeadDocuments.CountAsync(d => d.VerifiedBy == userId)
                });
            }

            var vm = new TeamDetailsViewModel
            {
                TeamName = team.TeamName,
                Members = memberStats
            };

            return View(vm);
            //         }public async Task<IActionResult> Members(int id)
            // {
            //     var team = await _context.Teams
            //         .Include(t => t.Members)
            //             .ThenInclude(m => m.User)   // 🔥 ensure User is included
            //         .FirstOrDefaultAsync(t => t.TeamId == id);

            //     if (team == null)
            //     {
            //         return NotFound();
            //     }

            //     return View(team);
        }

    }
}
