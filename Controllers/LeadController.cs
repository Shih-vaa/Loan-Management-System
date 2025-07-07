using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LoanManagementSystem.Data;
using LoanManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Controllers
{
    [Authorize]
    public class LeadController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LeadController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            string role = HttpContext.Session.GetString("UserRole") ?? "";
            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var leads = _context.Leads
                .Include(l => l.Customer)
                .Include(l => l.LeadGenerator)
                .Include(l => l.AssignedUser)
                .AsQueryable();

            if (role == "marketing")
                leads = leads.Where(l => l.LeadGeneratorId == userId);
            else if (role != "admin")
                leads = leads.Where(l => l.AssignedTo == userId);

            return View(await leads.ToListAsync());
        }

        [Authorize(Roles = "admin,marketing")]
        public IActionResult Create()
        {
            ViewBag.Customers = _context.Customers.ToList();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "admin,marketing")]
        public async Task<IActionResult> Create(Lead lead)
        {
            lead.LeadGeneratorId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
            lead.CreatedAt = DateTime.Now;
            _context.Leads.Add(lead);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "admin")]

        public async Task<IActionResult> Edit(int id)
        {
            var lead = await _context.Leads
                .Include(l => l.Customer)
                .Include(l => l.LeadGenerator)
                .Include(l => l.AssignedUser)
                .FirstOrDefaultAsync(l => l.LeadId == id);

            if (lead == null) return NotFound();

            ViewBag.Users = await _context.Users
                .Where(u => u.Role == "calling" || u.Role == "office") // ✅ Only assignable roles
                .ToListAsync();

            return View(lead);
        }


        [HttpPost]
        [Authorize(Roles = "admin")]


        public async Task<IActionResult> Edit(Lead updatedLead)
        {
            var lead = await _context.Leads.FindAsync(updatedLead.LeadId);
            if (lead == null) return NotFound();

            lead.Status = updatedLead.Status;
            lead.AssignedTo = updatedLead.AssignedTo;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Details(int id)
        {
            var lead = await _context.Leads
                .Include(l => l.Customer)
                .Include(l => l.LeadGenerator)
                .Include(l => l.AssignedUser)
                .FirstOrDefaultAsync(l => l.LeadId == id);

            return lead == null ? NotFound() : View(lead);
        }
    }
}
