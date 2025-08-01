using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoanManagementSystem.Data;
using LoanManagementSystem.Models;
using LoanManagementSystem.Models.ViewModels;
using LoanManagementSystem.Helpers;
using Microsoft.Extensions.Logging;

namespace LoanManagementSystem.Controllers
{
    [Authorize]
    public class LeadController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LeadController> _logger;
        private readonly IEmailHelper _emailHelper;

        public LeadController(ApplicationDbContext context, ILogger<LeadController> logger, IEmailHelper emailHelper)
        {
            _context = context;
            _logger = logger;
            _emailHelper = emailHelper;
        }

        public async Task<IActionResult> Index(string? status = null)
        {
            string role = HttpContext.Session.GetString("UserRole") ?? "";
            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var leads = _context.Leads
                .Include(l => l.Customer)
                .Include(l => l.LeadGenerator)
                .Include(l => l.AssignedUser)
                .Where(l => !l.IsDeleted);

            if (!string.IsNullOrEmpty(status))
                leads = leads.Where(l => l.Status == status);

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

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var lead = await _context.Leads.Where(l => l.LeadId == id && !l.IsDeleted).FirstOrDefaultAsync();

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditLeadViewModel model)
        {
            if (id != model.LeadId) return NotFound();

            var lead = await _context.Leads.FindAsync(id);
            if (lead == null || lead.IsDeleted) return NotFound();

            string auditDescription = "";
            bool isNewAssignment = lead.AssignedTo != model.AssignedTo;

            lead.Status = model.Status;

           if (isNewAssignment)
{
    string? previousAssigneeName = null;
    int? previousAssigneeId = null;

    if (lead.AssignedTo.HasValue)
    {
        var previousUser = await _context.Users.FindAsync(lead.AssignedTo.Value);
        previousAssigneeName = previousUser?.FullName ?? $"UserId {lead.AssignedTo.Value}";
        previousAssigneeId = previousUser?.UserId;
    }

    if (model.AssignedTo.HasValue)
    {
        var assignedUser = await _context.Users.FindAsync(model.AssignedTo.Value);

        if (assignedUser != null)
        {
            lead.AssignedTo = assignedUser.UserId;

            bool isReassignment = previousAssigneeId.HasValue;

            auditDescription = isReassignment
                ? $"Lead LMS-{lead.LeadId:D4} reassigned from {previousAssigneeName} to {assignedUser.FullName} (ID: {assignedUser.UserId})."
                : $"Lead LMS-{lead.LeadId:D4} assigned to {assignedUser.FullName} (ID: {assignedUser.UserId}).";

            // Send appropriate email
            if (isReassignment)
            {
                await _emailHelper.SendLeadReassignmentEmailAsync(
                    assignedUser.Email!,
                    assignedUser.FullName,
                    lead.LeadId
                );
            }
            else
            {
                await _emailHelper.SendLeadAssignmentEmailAsync(
                    assignedUser.Email!,
                    assignedUser.FullName,
                    lead.LeadId
                );
            }

            // Notifications
            await NotificationHelper.AddNotificationAsync(
                _context,
                assignedUser.UserId,
                $"New Lead Assigned: LMS-{lead.LeadId:D4}"
            );

            if (lead.LeadGeneratorId != assignedUser.UserId)
            {
                await NotificationHelper.AddNotificationAsync(
                    _context,
                    lead.LeadGeneratorId,
                    $"Your lead LMS-{lead.LeadId:D4} has been assigned"
                );
            }

            TempData["Success"] = $"Lead LMS-{lead.LeadId:D4} successfully {(isReassignment ? "reassigned" : "assigned")} to {assignedUser.FullName} ({assignedUser.Email}).";
        }
        else
        {
            auditDescription = $"Lead LMS-{lead.LeadId:D4} reassigned.";
        }
    }
    else
    {
        lead.AssignedTo = null;
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

                    _logger.LogInformation($"Commission created for Lead {lead.LeadId}: ₹{commissionAmount}");
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var lead = await _context.Leads
                .Include(l => l.Customer)
                .Include(l => l.LeadGenerator)
                .Include(l => l.AssignedUser)
                .Include(l => l.Documents!).ThenInclude(d => d.UploadedByUser)
                .Include(l => l.Documents!).ThenInclude(d => d.Verifier)
                .FirstOrDefaultAsync(l => l.LeadId == id && !l.IsDeleted);

            return lead == null ? NotFound() : View(lead);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var lead = await _context.Leads
                .Include(l => l.Customer)
                .Include(l => l.AssignedUser)
                .FirstOrDefaultAsync(l => l.LeadId == id && !l.IsDeleted);

            return lead == null ? NotFound() : View(lead);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Approve(int leadId, string status, string? remarks)
        {
            var lead = await _context.Leads
                .Where(l => l.LeadId == leadId && !l.IsDeleted)
                .FirstOrDefaultAsync();

            if (lead == null) return NotFound();

            if (status != "approved" && status != "disbursed")
                return BadRequest("Invalid status");

            lead.Status = status;
            lead.Remarks = remarks;

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

                await NotificationHelper.AddNotificationAsync(_context,
                    lead.AssignedTo.Value,
                    $"Lead LMS-{leadId:D4} has been marked as {status.ToUpper()} by admin.");

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

        private decimal CalculateCommissionAmount(decimal loanAmount)
        {
            return loanAmount * 0.02M;
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var lead = await _context.Leads
                .Include(l => l.Customer)
                .FirstOrDefaultAsync(l => l.LeadId == id);

            if (lead == null || lead.IsDeleted) return NotFound();

            lead.IsDeleted = true;
            lead.DateDeleted = DateTime.UtcNow;

            if (lead.Customer != null)
            {
                lead.Customer.FullName = "Deleted";
                lead.Customer.Email = null;
                lead.Customer.Phone = null;
            }

            await AuditLogger.LogAsync(
                _context,
                HttpContext,
                action: "Lead Deletion",
                description: $"Lead LMS-{lead.LeadId:D4} was anonymized and soft deleted.",
                controller: "Lead",
                actionMethod: "Delete"
            );

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Lead LMS-{lead.LeadId:D4} has been deleted.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeletedLeads()
        {
            var deletedLeads = await _context.Leads
                .Where(l => l.IsDeleted)
                .OrderByDescending(l => l.DateDeleted)
                .ToListAsync();

            return View(deletedLeads);
        }
    }
}