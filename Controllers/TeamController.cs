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
            ViewBag.AllUsers = await _context.Users
                .Where(u => !currentUserIds.Contains(u.UserId))
                .ToListAsync();

            return View(team);
        }












        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddMember(int teamId, int userId, bool canManageLeads = false, bool canUploadDocs = false, bool canVerifyDocs = false)
        {
            // Prevent duplicates
            bool alreadyMember = await _context.TeamMembers.AnyAsync(tm => tm.TeamId == teamId && tm.UserId == userId);
            if (alreadyMember)
            {
                TempData["Error"] = "User is already a member of this team.";
                return RedirectToAction("Members", new { id = teamId });
            }

            var newMember = new TeamMember
            {
                TeamId = teamId,
                UserId = userId,
                CanManageLeads = canManageLeads,
                CanUploadDocs = canUploadDocs,
                CanVerifyDocs = canVerifyDocs
            };

            _context.TeamMembers.Add(newMember);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Member added successfully with permissions.";
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
    }
}
