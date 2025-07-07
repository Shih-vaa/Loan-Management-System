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

        // POST: Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int leadId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "No file selected.";
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
                UploadedAt = DateTime.Now
            };

            _context.LeadDocuments.Add(doc);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Document uploaded successfully.";
            return RedirectToAction("Details", "Lead", new { id = leadId });
        }

        // GET: View all documents for a lead
        [HttpGet]
        public async Task<IActionResult> LeadDocs(int leadId)
        {
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
