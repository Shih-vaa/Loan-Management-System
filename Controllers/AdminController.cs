using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            ViewBag.UserEmail = User.Identity?.Name;
            return View();
        }
    }
}
