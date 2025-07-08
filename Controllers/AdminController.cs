using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LoanManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

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

    public IActionResult Dashboard()
{
    int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

    var teamMemberships = _context.TeamMembers
        .Include(tm => tm.Team)
        .Include(tm => tm.User)  // Include self and others
        .Where(tm => tm.UserId == userId)
        .ToList();

    var teamIds = teamMemberships.Select(tm => tm.TeamId).ToList();

    // Fetch all teammates from the same teams (excluding self)
    var teammates = _context.TeamMembers
        .Include(tm => tm.User)
        .Include(tm => tm.Team)
        .Where(tm => teamIds.Contains(tm.TeamId))
        .ToList();

    ViewBag.Name = HttpContext.Session.GetString("UserName");
    ViewBag.TeamMemberships = teamMemberships.Select(tm => tm.Team).Distinct().ToList();
    ViewBag.Teammates = teammates;

    return View();
}

    }
}
