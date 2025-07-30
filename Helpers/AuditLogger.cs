using LoanManagementSystem.Data;
using LoanManagementSystem.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace LoanManagementSystem.Helpers
{
    public static class AuditLogger
    {
        public static async Task LogAsync(
    ApplicationDbContext context,
    HttpContext httpContext,
    string action,
    string description,
    string controller,
    string actionMethod,
    int? userIdOverride = null,
    string? roleOverride = null
)
        {
            var log = new AuditLog
            {
                Action = action,
                Description = description,
                Controller = controller,
                ActionMethod = actionMethod,
                UserId = userIdOverride ??
                         (httpContext.User.Identity?.IsAuthenticated == true
                            ? int.Parse(httpContext.User.FindFirst("UserId")?.Value ?? "0")
                            : null),
                Role = roleOverride ??
                       (httpContext.User.Identity?.IsAuthenticated == true
                            ? httpContext.User.FindFirst(ClaimTypes.Role)?.Value
                            : null),
                IPAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = httpContext.Request.Headers["User-Agent"].ToString(),
                Timestamp = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")
        )

            };

            context.AuditLogs.Add(log);
            await context.SaveChangesAsync();
        }

    }
}
