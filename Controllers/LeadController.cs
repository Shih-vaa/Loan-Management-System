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

        // ✅ GET: /Lead (with optional status filter)
        public async Task<IActionResult> Index(string? status = null)
        {
            string role = HttpContext.Session.GetString("UserRole") ?? "";
            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var leads = _context.Leads
                .Include(l => l.Customer)
                .Include(l => l.LeadGenerator)
                .Include(l => l.AssignedUser)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                leads = leads.Where(l => l.Status == status);
            }

            if (role == "marketing")
                leads = leads.Where(l => l.LeadGeneratorId == userId);
            else if (role != "admin")
                leads = leads.Where(l => l.AssignedTo == userId);

            return View(await leads.ToListAsync());
        }

        // ✅ GET: /Lead/Create
        [Authorize(Roles = "admin,marketing")]
        public IActionResult Create()
        {
            ViewBag.Customers = _context.Customers.ToList();
            return View();
        }

        // ✅ POST: /Lead/Create
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

        // ✅ GET: /Lead/Edit/{id}
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

        // ✅ POST: /Lead/Edit
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

            // ✅ Auto-create commission
            if ((model.Status == "approved" || model.Status == "disbursed") && model.AssignedTo.HasValue)
            {
                bool commissionExists = await _context.Commissions
                    .AnyAsync(c => c.LeadId == lead.LeadId);

                if (!commissionExists)
                {
                    if (lead.LoanAmount <= 0)
                    {
                        Console.WriteLine("⚠️ Loan amount is zero. Defaulting to 500000");
                        lead.LoanAmount = 500000;
                    }

                    decimal commissionAmount = CalculateCommissionAmount(lead.LoanAmount);

                    _context.Commissions.Add(new Commission
                    {
                        LeadId = lead.LeadId,
                        UserId = model.AssignedTo.Value,
                        Amount = commissionAmount,
                        Status = "pending",
                        CalculatedAt = DateTime.UtcNow
                    });

                    Console.WriteLine($"💰 Commission created: ₹{commissionAmount} for Lead {lead.LeadId}");
                }
            }
            if (lead.AssignedTo != model.AssignedTo)
            {
                lead.AssignedAt = DateTime.UtcNow; // ✅ Timestamp assignment
            }



            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ✅ GET: /Lead/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var lead = await _context.Leads
                .Include(l => l.Customer)
                .Include(l => l.LeadGenerator)
                .Include(l => l.AssignedUser)
                .Include(l => l.Documents!)
                    .ThenInclude(d => d.UploadedByUser)
                .Include(l => l.Documents!)
                    .ThenInclude(d => d.Verifier)
                .FirstOrDefaultAsync(l => l.LeadId == id);

            return lead == null ? NotFound() : View(lead);
        }

        // ✅ Commission logic
        private decimal CalculateCommissionAmount(decimal loanAmount)
        {
            return loanAmount * 0.02M; // Example: 2%
        }


    }
}
