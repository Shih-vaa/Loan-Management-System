using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoanManagementSystem.Data;
using LoanManagementSystem.Models;
using LoanManagementSystem.Models.ViewModels;
using LoanManagementSystem.Helpers; // ✅ For NotificationHelper
using Microsoft.Extensions.Logging;



namespace LoanManagementSystem.Controllers
{
    [Authorize]
    public class LeadController : Controller
    {
        private readonly ApplicationDbContext _context;





        private readonly ILogger<LeadController> _logger;



        private readonly EmailHelper _emailHelper;
        public LeadController(ApplicationDbContext context, ILogger<LeadController> logger, EmailHelper emailHelper)
        {
            _context = context;
            _logger = logger;
            _emailHelper = emailHelper;
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
                leads = leads.Where(l => l.Status == status);

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
            lead.CreatedAt = DateTime.UtcNow;

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



        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(EditLeadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Users = await _context.Users
                    .Where(u => u.Role == "calling" || u.Role == "office")
                    .ToListAsync();
                return View(model);
            }

            var lead = await _context.Leads.FindAsync(model.LeadId);
            if (lead == null) return NotFound();

            bool isNewAssignment = model.AssignedTo.HasValue && lead.AssignedTo != model.AssignedTo;

            lead.Status = model.Status;
            lead.AssignedTo = model.AssignedTo;

            string auditDescription = null;

            if (isNewAssignment)
            {
                // ✅ Audit logging for assignment changes
                string previousAssigneeName = null;

                if (lead.AssignedTo.HasValue)
                {
                    var previousUser = await _context.Users.FindAsync(lead.AssignedTo.Value);
                    previousAssigneeName = previousUser?.FullName ?? $"UserId {lead.AssignedTo.Value}";
                }

                if (model.AssignedTo.HasValue)
                {
                    var assignedUser = await _context.Users.FindAsync(model.AssignedTo.Value);

                    if (assignedUser != null)
                    {
                        if (previousAssigneeName == null)
                        {
                            auditDescription = $"Lead LMS-{lead.LeadId:D4} assigned to {assignedUser.FullName} (ID: {assignedUser.UserId}).";
                        }
                        else
                        {
                            auditDescription = $"Lead LMS-{lead.LeadId:D4} reassigned from {previousAssigneeName} to {assignedUser.FullName} (ID: {assignedUser.UserId}).";
                        }
                    }
                    else
                    {
                        auditDescription = $"Lead LMS-{lead.LeadId:D4} reassigned.";
                    }
                }
                else
                {
                    auditDescription = $"Lead LMS-{lead.LeadId:D4} was unassigned from {previousAssigneeName}.";
                }

                await AuditLogger.LogAsync(
                    _context,
                    HttpContext,
                    action: "Lead Assignment Update",
                    description: auditDescription,
                    controller: "Lead",
                    actionMethod: "Edit"
                );
            }

            // ✅ Commission generation
            if ((model.Status == "approved" || model.Status == "disbursed") && model.AssignedTo.HasValue)
            {
                bool commissionExists = await _context.Commissions.AnyAsync(c => c.LeadId == lead.LeadId);
                if (!commissionExists)
                {
                    if (lead.LoanAmount <= 0)
                    {
                        _logger.LogWarning("Loan amount is zero. Defaulting to ₹5L");
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

                    _logger.LogInformation($"💰 Commission created for Lead {lead.LeadId}: ₹{commissionAmount}");
                }
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







        // GET: Lead/Approve/{id}
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var lead = await _context.Leads
                .Include(l => l.Customer)
                .Include(l => l.AssignedUser)
                .FirstOrDefaultAsync(l => l.LeadId == id);

            if (lead == null) return NotFound();
            return View(lead);
        }




        // POST: Lead/Approve
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Approve(int leadId, string status, string? remarks)
        {
            var lead = await _context.Leads.FindAsync(leadId);
            if (lead == null) return NotFound();

            if (status != "approved" && status != "disbursed")
                return BadRequest("Invalid status");

            lead.Status = status;
            lead.Remarks = remarks;

            // ✅ Commission logic
            if (lead.AssignedTo.HasValue)
            {
                bool exists = await _context.Commissions.AnyAsync(c => c.LeadId == leadId);
                if (!exists)
                {
                    decimal commission = lead.LoanAmount > 0 ? lead.LoanAmount * 0.02M : 500000 * 0.02M;
                    _context.Commissions.Add(new Commission
                    {
                        LeadId = leadId,
                        UserId = lead.AssignedTo.Value,
                        Amount = commission,
                        Status = "pending",
                        CalculatedAt = DateTime.UtcNow
                    });
                }

                // ✅ Notify assigned calling user
                await NotificationHelper.AddNotificationAsync(_context,
                    lead.AssignedTo.Value,
                    $"Lead LMS-{leadId:D4} has been marked as {status.ToUpper()} by admin.");

                // ✅ Notify marketing agent
                if (lead.LeadGeneratorId != lead.AssignedTo.Value)
                {
                    await NotificationHelper.AddNotificationAsync(_context,
                        lead.LeadGeneratorId,
                        $"Your lead LMS-{leadId:D4} has been approved by admin.");
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Lead LMS-{leadId:D4} marked as {status}.";
            return RedirectToAction("Index");
        }






        // ✅ Commission formula
        private decimal CalculateCommissionAmount(decimal loanAmount)
        {
            return loanAmount * 0.02M; // 2% commission
        }

    }

}
