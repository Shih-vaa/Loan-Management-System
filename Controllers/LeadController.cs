using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoanManagementSystem.Data;
using LoanManagementSystem.Models;
using LoanManagementSystem.Models.ViewModels;
using Microsoft.Extensions.Logging;

namespace LoanManagementSystem.Controllers
{
    [Authorize]
    public class LeadController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LeadController> _logger;

        public LeadController(ApplicationDbContext context, ILogger<LeadController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Lead
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

        // GET: /Lead/Create
        [Authorize(Roles = "admin,marketing")]
        public IActionResult Create()
        {
            ViewBag.Customers = _context.Customers.ToList();
            return View();
        }

        // POST: /Lead/Create
        [HttpPost]
        [Authorize(Roles = "admin,marketing")]
        public async Task<IActionResult> Create(Lead lead)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Customers = _context.Customers.ToList();
                return View(lead);
            }

            lead.LeadGeneratorId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
            lead.CreatedAt = DateTime.Now;

            _context.Leads.Add(lead);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Lead/Edit/{id}
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var lead = await _context.Leads.FindAsync(id);
            if (lead == null) return NotFound();

            var model = new EditLeadViewModel
            {
                LeadId = lead.LeadId,
                Status = lead.Status,
                AssignedTo = lead.AssignedTo
            };

            ViewBag.Users = await _context.Users
                .Where(u => u.Role == "calling" || u.Role == "office")
                .ToListAsync();

            return View(model);
        }

        // POST: /Lead/Edit
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(EditLeadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"🔴 {error.ErrorMessage}");
                }

                ViewBag.Users = await _context.Users
                    .Where(u => u.Role == "calling" || u.Role == "office")
                    .ToListAsync();

                return View(model);
            }

            var lead = await _context.Leads.FindAsync(model.LeadId);
            if (lead == null) return NotFound();

            lead.Status = model.Status;
            lead.AssignedTo = model.AssignedTo;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Lead/Details/{id}
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
