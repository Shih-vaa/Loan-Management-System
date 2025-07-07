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
            var teams = await _context.Teams.Include(t => t.Creator).ToListAsync();
            return View(teams);
        }

        // GET: /Team/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Team/Create
        [HttpPost]
        public async Task<IActionResult> Create(Team team)
        {
            team.CreatedBy = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Team/Members/5
        public async Task<IActionResult> Members(int id)
        {
            var team = await _context.Teams.Include(t => t.Members).ThenInclude(m => m.User)
                                           .FirstOrDefaultAsync(t => t.TeamId == id);
            if (team == null) return NotFound();

            ViewBag.AllUsers = await _context.Users.ToListAsync();
            return View(team);
        }

        // POST: /Team/AddMember
        [HttpPost]
        public async Task<IActionResult> AddMember(int teamId, int userId)
        {
            var exists = await _context.TeamMembers.FindAsync(teamId, userId);
            if (exists == null)
            {
                _context.TeamMembers.Add(new TeamMember { TeamId = teamId, UserId = userId });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Members", new { id = teamId });
        }

        // POST: /Team/RemoveMember
        [HttpPost]
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
