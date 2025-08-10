using LoanManagementSystem.Data;
using LoanManagementSystem.Helpers;
using LoanManagementSystem.Models; // <-- Your Message model
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // <-- Needed for Include/ToListAsync
using System.Linq;
using System.Threading.Tasks;

[Authorize]
public class MessagesController : Controller
{
    private readonly ApplicationDbContext _context;

    public MessagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Inbox
    public async Task<IActionResult> Index()
    {
        int userId = int.Parse(User.FindFirst("UserId")!.Value);
        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Where(m => m.RecipientId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return View(messages);
    }

    // Compose
    [HttpGet]
    public IActionResult Create()
    {
        int senderId = int.Parse(User.FindFirst("UserId")!.Value);
        string role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)!.Value;

        // Base query: exclude yourself
        var query = _context.Users.Where(u => u.UserId != senderId);

        if (role == "admin")
        {
            // Admin can send to anyone except themselves
            ViewBag.Users = query.ToList();
        }
        else if (role == "marketing")
        {
            // Marketing can send to admin or calling in same team
            int? teamId = _context.TeamMembers
                .Where(t => t.UserId == senderId)
                .Select(t => t.TeamId)
                .FirstOrDefault();

            ViewBag.Users = query.Where(u =>
                u.Role == "admin" ||
                (u.Role == "calling" && _context.TeamMembers.Any(tm => tm.TeamId == teamId && tm.UserId == u.UserId))
            ).ToList();
        }
        else if (role == "calling")
        {
            // Calling can send to admin or office in same team
            int? teamId = _context.TeamMembers
                .Where(t => t.UserId == senderId)
                .Select(t => t.TeamId)
                .FirstOrDefault();

            ViewBag.Users = query.Where(u =>
                u.Role == "admin" ||
                (u.Role == "office" && _context.TeamMembers.Any(tm => tm.TeamId == teamId && tm.UserId == u.UserId))
            ).ToList();
        }
        else if (role == "office")
        {
            // Office can send to admin or calling in same team
            int? teamId = _context.TeamMembers
                .Where(t => t.UserId == senderId)
                .Select(t => t.TeamId)
                .FirstOrDefault();

            ViewBag.Users = query.Where(u =>
                u.Role == "admin" ||
                (u.Role == "calling" && _context.TeamMembers.Any(tm => tm.TeamId == teamId && tm.UserId == u.UserId))
            ).ToList();
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(int recipientId, string subject, string body)
    {
        int senderId = int.Parse(User.FindFirst("UserId")!.Value);

        var message = new Message
        {
            SenderId = senderId,
            RecipientId = recipientId,
            Subject = subject,
            Body = body
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // Optional: Add to notifications
        await NotificationHelper.AddNotificationAsync(
            _context,
            recipientId,
            $"New message from {User.Identity.Name}"
        );

        return RedirectToAction("Index");
    }

    // View message
    public async Task<IActionResult> Details(int id)
    {
        var message = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Recipient)
            .FirstOrDefaultAsync(m => m.MessageId == id);

        if (message == null) return NotFound();

        // Mark as read
        if (!message.IsRead)
        {
            message.IsRead = true;
            await _context.SaveChangesAsync();
        }

        return View(message);
    }
}
