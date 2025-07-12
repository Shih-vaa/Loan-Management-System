using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoanManagementSystem.Data;
using LoanManagementSystem.Helpers;
using System.Linq;
using System.Threading.Tasks;

namespace LoanManagementSystem.Controllers
{
    [Authorize(Roles = "office")]
    public class OfficeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OfficeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "office")]
        public async Task<IActionResult> Dashboard()
        {
            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
            var (teams, teammates) = TeamHelper.GetUserTeams(_context, userId);

            ViewBag.Name = HttpContext.Session.GetString("UserName");
            ViewBag.TeamMemberships = teams;
            ViewBag.Teammates = teammates;

            var leads = await _context.Leads
                .Include(l => l.Customer)
                .Include(l => l.Documents)
                .Where(l => l.Status == "new") // Make sure this matches your Lead status
                .ToListAsync();

            return View(leads);
        }
    }
}