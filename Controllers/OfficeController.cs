using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.Controllers
{
    [Authorize(Roles = "office")]
    public class OfficeController : Controller
    {
        public IActionResult Dashboard()
        {
            ViewBag.Name = HttpContext.Session.GetString("UserName");
            return View();
        }
    }
}
