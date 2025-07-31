using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoanManagementSystem.Data;
using LoanManagementSystem.Models;
using LoanManagementSystem.Helpers;

namespace LoanManagementSystem.Controllers
{
    [Authorize(Roles = "calling")]
    public class CallingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CallingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var leads = await _context.Leads
            .Include(l => l.Customer)
            .Include(l => l.Documents) // 👈 Needed for showing rejected docs
            .Where(l => l.AssignedTo == userId && !l.IsDeleted)
            .ToListAsync();

            return View(leads);
        }

        public async Task<IActionResult> UploadDocuments(int id)
        {
            var lead = await _context.Leads
                .Include(l => l.Customer)
                .FirstOrDefaultAsync(l => l.LeadId == id && !l.IsDeleted);

            if (lead == null) return NotFound();

            ViewBag.Lead = lead;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadDocuments(int id, List<IFormFile> files, List<string> types)
        {
            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var type = types[i];

                if (file != null && file.Length > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var filePath = Path.Combine("wwwroot/uploads", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    _context.LeadDocuments.Add(new LeadDocument
                    {
                        LeadId = id,
                        DocumentType = type,
                        FileName = fileName,
                        FilePath = "/uploads/" + fileName,
                        UploadedAt = DateTime.UtcNow,
                        UploadedBy = userId,
                        Status = DocumentStatus.Pending
                    });
                }
            }

            var lead = await _context.Leads.FindAsync(id);
            string oldStatus = lead.Status;

            lead.Status = "in_process";
            Console.WriteLine("Logging audit...");
            // ✅ Audit Log
            await AuditLogger.LogAsync(
                _context,
                HttpContext,
                action: "Lead Status Change",
                description: $"Status of LMS-{lead.LeadId:D4} changed from '{oldStatus}' to 'in-process' due to document upload.",
                controller: "Calling",
                actionMethod: "UploadDocument"
            );
            Console.WriteLine("Audit logged!");

            // Notify office team
            var officeUsers = await _context.Users
                .Where(u => u.Role == "office")
                .ToListAsync();

            foreach (var officeUser in officeUsers)
            {
                await NotificationHelper.AddNotificationAsync(
                    _context,
                    officeUser.UserId,
                    $"Documents uploaded for Lead LMS-{lead.LeadId:D4}");
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }
        public async Task<IActionResult> ViewRejectedDocs(int leadId)
        {
            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var lead = await _context.Leads
                .Include(l => l.Customer)
                .Include(l => l.Documents!)
                .ThenInclude(d => d.Verifier)
                .FirstOrDefaultAsync(l => l.LeadId == leadId && l.AssignedTo == userId && !l.IsDeleted);

            if (lead == null) return NotFound();

            var rejectedDocs = lead.Documents
                .Where(d => d.Status == DocumentStatus.Rejected)
                .ToList();

            ViewBag.Lead = lead;
            return View("RejectedDocs", rejectedDocs);
        }



        [HttpPost]
        [Authorize(Roles = "calling")]
        public async Task<IActionResult> ReuploadDocuments(int leadId, List<IFormFile> files, List<int> documentIds, List<string> types)
        {
            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            if (files.Count != documentIds.Count || files.Count != types.Count)
            {
                TempData["Error"] = "Mismatch in uploaded file data.";
                return RedirectToAction("RejectedDocs", new { leadId });
            }

            var uploadsFolder = Path.Combine("wwwroot/uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var docId = documentIds[i];
                var type = types[i];

                var doc = await _context.LeadDocuments.FindAsync(docId);
                if (doc == null || doc.LeadId != leadId || doc.Status != DocumentStatus.Rejected)
                    continue;

                // ❌ Optional: Delete old file
                var oldPath = Path.Combine("wwwroot", doc.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);

                // ✅ Save new file
                var newFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var newFilePath = Path.Combine(uploadsFolder, newFileName);

                using (var stream = new FileStream(newFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // ✅ Update document
                doc.FileName = file.FileName;
                doc.FilePath = "/uploads/" + newFileName;
                doc.UploadedAt = DateTime.UtcNow;
                doc.UploadedBy = userId;
                doc.Status = DocumentStatus.Pending;
                doc.Remarks = null;
                doc.VerifiedAt = null;
                doc.VerifiedBy = null;
            }

            // ✅ Notify office member from same team
var lead = await _context.Leads.FirstOrDefaultAsync(l => l.LeadId == leadId && !l.IsDeleted);

            if (lead != null && lead.AssignedTo.HasValue)
            {
                var officeUsers = TeamHelper.GetOfficeMembersInSameTeam(_context, userId);
                foreach (var office in officeUsers)
                {
                    await NotificationHelper.AddNotificationAsync(
                        _context,
                        office.UserId,
                        $"Re-uploaded documents for Lead LMS-{leadId:D4} are ready for verification.");
                }

                lead.Status = "in_process";
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Documents re-uploaded successfully.";
            return RedirectToAction("Dashboard");
        }


    }
}
