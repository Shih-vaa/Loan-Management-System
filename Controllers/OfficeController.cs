using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LoanManagementSystem.Data;
using LoanManagementSystem.Helpers;

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

        public IActionResult Dashboard()
        {
            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
            var (teams, teammates) = TeamHelper.GetUserTeams(_context, userId);

            ViewBag.Name = HttpContext.Session.GetString("UserName");
            ViewBag.TeamMemberships = teams;
            ViewBag.Teammates = teammates;

            return View();
        }
    }
}
