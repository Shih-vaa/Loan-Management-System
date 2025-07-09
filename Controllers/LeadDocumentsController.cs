using LoanManagementSystem.Data;
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








        // GET: Review Page (Admin or Office sees list of documents with approve/reject buttons)
        // GET: Review
        [Authorize(Roles = "admin,office")]
        public async Task<IActionResult> Review(int leadId)
        {
            var currentUserId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var lead = await _context.Leads.FindAsync(leadId);
            if (lead == null) return NotFound();

            // 🔐 Office can only view their assigned leads
            if (User.IsInRole("office") && lead.AssignedTo != currentUserId)
            {
                return Forbid(); // Access denied
            }

            var docs = await _context.LeadDocuments
                .Where(d => d.LeadId == leadId)
                .Include(d => d.UploadedByUser)
                .Include(d => d.Verifier)
                .ToListAsync();

            ViewBag.LeadId = leadId;
            return View(docs);
        }












        // POST: Approve / Reject Document
        [HttpPost]
        [Authorize(Roles = "admin,office")]
        public async Task<IActionResult> Verify(int id, string status, string? remarks)
        {
            var doc = await _context.LeadDocuments.FindAsync(id);
            if (doc == null) return NotFound();

            if (status != "approved" && status != "rejected")
                return BadRequest("Invalid status");

            doc.Status = status;
            doc.VerifiedBy = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
            doc.VerifiedAt = DateTime.UtcNow;
            doc.Remarks = remarks;
            await _context.SaveChangesAsync();
            return RedirectToAction("Review", new { leadId = doc.LeadId });
        }













        // POST: Upload with DocumentType and default Status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int leadId, string documentType, IFormFile file)
        {
            if (file == null || file.Length == 0 || string.IsNullOrWhiteSpace(documentType))
            {
                TempData["Error"] = "Please select a file and document type.";
                return RedirectToAction("Create", new { leadId });
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

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
                UploadedBy = int.Parse(User.Claims.First(c => c.Type == "UserId").Value),
                UploadedAt = DateTime.Now,
                DocumentType = documentType,
                Status = "pending"  // default status = pending
            };

            _context.LeadDocuments.Add(doc);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Document uploaded successfully.";
            return RedirectToAction("Details", "Lead", new { id = leadId });
        }












        // GET: View all the LeadDocs
        [Authorize(Roles = "admin,calling,office")]
        public async Task<IActionResult> LeadDocs(int leadId)
        {
            var currentUserId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var lead = await _context.Leads.FindAsync(leadId);
            if (lead == null) return NotFound();

            // 🔐 Calling or Office can only view their assigned leads
            if (!User.IsInRole("admin") && lead.AssignedTo != currentUserId)
            {
                return Forbid();
            }

            var docs = await _context.LeadDocuments
                .Where(d => d.LeadId == leadId)
                .Include(d => d.UploadedByUser)
                .ToListAsync();

            ViewBag.LeadId = leadId;
            return View(docs);
        }











        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var doc = await _context.LeadDocuments.FindAsync(id);
            if (doc == null) return NotFound();

            // Delete the file from wwwroot/uploads
            var filePath = Path.Combine(_env.WebRootPath, "uploads", Path.GetFileName(doc.FilePath));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.LeadDocuments.Remove(doc);
            await _context.SaveChangesAsync();

            return RedirectToAction("LeadDocs", new { leadId = doc.LeadId });
        }







    }
}
