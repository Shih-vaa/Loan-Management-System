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
                .Where(l => l.AssignedTo == userId)
                .ToListAsync();

            return View(leads);
        }

        public async Task<IActionResult> UploadDocuments(int id)
        {
            var lead = await _context.Leads
                .Include(l => l.Customer)
                .FirstOrDefaultAsync(l => l.LeadId == id);

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
            lead.Status = "in_process";

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
    }
}
