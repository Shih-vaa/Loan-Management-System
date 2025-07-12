using LoanManagementSystem.Data;
using LoanManagementSystem.Helpers;
using LoanManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Controllers
{
    [Authorize(Roles = "admin,calling,office")]
    public class LeadDocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public LeadDocumentsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }















        // GET: Upload form
        public IActionResult Create(int leadId)
        {
            ViewBag.LeadId = leadId;
            return View();
        }






















        // POST: Upload document
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int leadId, string documentType, IFormFile file)
        {
            if (file == null || file.Length == 0 || string.IsNullOrWhiteSpace(documentType))
            {
                TempData["Error"] = "Please select a file and document type.";
                return RedirectToAction("Create", new { leadId });
            }

            var currentUserId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
            bool canUpload = User.IsInRole("admin") || PermissionHelper.HasTeamPermission(_context, HttpContext, "CanUploadDocs");

            if (!canUpload)
            {
                return Forbid(); // ❌ not allowed to upload
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var doc = new LeadDocument
            {
                LeadId = leadId,
                FileName = file.FileName,
                FilePath = "/uploads/" + fileName,
                UploadedBy = currentUserId,
                UploadedAt = DateTime.UtcNow,
                DocumentType = documentType,
                Status = DocumentStatus.Pending
            };

            _context.LeadDocuments.Add(doc);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Document uploaded successfully.";
            return RedirectToAction("Details", "Lead", new { id = leadId });
        }















        // GET: View uploaded docs (secure)
        [Authorize(Roles = "admin,calling,office")]
        public async Task<IActionResult> LeadDocs(int leadId)
        {
            var currentUserId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
            var lead = await _context.Leads.FindAsync(leadId);
            if (lead == null) return NotFound();

            if (!User.IsInRole("admin") && lead.AssignedTo != currentUserId)
                return Forbid(); // only assigned person can see

            var docs = await _context.LeadDocuments
                .Where(d => d.LeadId == leadId)
                .Include(d => d.UploadedByUser)
                .ToListAsync();

            ViewBag.LeadId = leadId;
            return View(docs);
        }















        // GET: Admin/Office document review page
        [Authorize(Roles = "admin,office")]
        public async Task<IActionResult> Review(int leadId)
        {
            var currentUserId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
            var lead = await _context.Leads.FindAsync(leadId);
            if (lead == null) return NotFound();

            if (User.IsInRole("office") && lead.AssignedTo != currentUserId)
                return Forbid(); // Office can only review their leads

            var canVerify = User.IsInRole("admin") || PermissionHelper.HasTeamPermission(_context, HttpContext, "CanVerifyDocs");
            if (!canVerify) return Forbid();

            var docs = await _context.LeadDocuments
                .Where(d => d.LeadId == leadId)
                .Include(d => d.UploadedByUser)
                .Include(d => d.Verifier)
                .ToListAsync();

            ViewBag.LeadId = leadId;
            return View(docs);
        }




















        // POST: Approve/Reject a document
        [HttpPost]
        [Authorize(Roles = "admin,office")]
        public async Task<IActionResult> Verify(int id, string status, string? remarks)
        {
            var doc = await _context.LeadDocuments.FindAsync(id);
            if (doc == null) return NotFound();

            // Convert to lowercase to match enum exactly
            status = status?.ToLower();

            if (!DocumentStatus.IsValid(status) ||
                (status != DocumentStatus.Approved && status != DocumentStatus.Rejected))
            {
                return BadRequest("Status must be either 'approved' or 'rejected'");
            }

            var currentUserId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
            var lead = await _context.Leads.FindAsync(doc.LeadId);

            if (User.IsInRole("office") && lead?.AssignedTo != currentUserId)
                return Forbid();

            bool canVerify = User.IsInRole("admin") || PermissionHelper.HasTeamPermission(_context, HttpContext, "CanVerifyDocs");
            if (!canVerify) return Forbid();

            doc.Status = status;
            doc.VerifiedBy = currentUserId;
            doc.VerifiedAt = DateTime.UtcNow;
            doc.Remarks = remarks;

            await _context.SaveChangesAsync();
            return RedirectToAction("Review", new { leadId = doc.LeadId });
        }

















        // POST: Delete a document (admin only)
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var doc = await _context.LeadDocuments.FindAsync(id);
            if (doc == null) return NotFound();

            var filePath = Path.Combine(_env.WebRootPath, "uploads", Path.GetFileName(doc.FilePath));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _context.LeadDocuments.Remove(doc);
            await _context.SaveChangesAsync();

            return RedirectToAction("LeadDocs", new { leadId = doc.LeadId });
        }
















        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AllDocs()
        {
            var docs = await _context.LeadDocuments
                .Include(d => d.UploadedByUser)
                .ToListAsync();
            return View("LeadDocs", docs); // Reuse the existing view
        }









        [Authorize(Roles = "admin")]
        public async Task<IActionResult> PendingDocs()
        {
            var docs = await _context.LeadDocuments
                .Where(d => d.Status == DocumentStatus.Pending)
                .Include(d => d.UploadedByUser)
                .ToListAsync();
            return View("LeadDocs", docs); // Reuse
        }









    }
}
