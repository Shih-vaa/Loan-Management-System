using LoanManagementSystem.Data;
using LoanManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class DashboardController : Controller
{
    private readonly LoanManagementSystem.Data.ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public DashboardController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var teamMember = await _context.TeamMembers
            .FirstOrDefaultAsync(tm => tm.UserId == currentUser.UserId);

        if (teamMember == null)
        {
            return Forbid();
        }

        // Redirect based on role and permissions
        if (currentUser.Role == "OfficeAgent" && 
            teamMember.CanManageLeads && 
            teamMember.CanUploadDocs && 
            teamMember.CanVerifyDocs)
        {
            return View("OfficeDashboard");
        }
        else if ((currentUser.Role == "MarketingAgent" || currentUser.Role == "CallingAgent") && 
                teamMember.CanUploadDocs)
        {
            return View(currentUser.Role + "Dashboard");
        }

        return Forbid();
    }
}