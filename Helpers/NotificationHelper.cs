using LoanManagementSystem.Data;
using LoanManagementSystem.Models;

namespace LoanManagementSystem.Helpers
{
    public static class NotificationHelper
    {
        // Add ILogger (if using DI) or throw exceptions
public static async Task AddNotificationAsync(
    ApplicationDbContext context, 
    int userId, 
    string message, 
    string? link = null,
    ILogger? logger = null) // Optional logger
{
    if (string.IsNullOrWhiteSpace(message)) return;

    var notification = new Notification
    {
        UserId = userId,
        Message = message,
        Link = link,
        IsRead = false,
        CreatedAt = DateTime.UtcNow
    };

    try
    {
        context.Notifications.Add(notification);
        await context.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        logger?.LogError(ex, "Failed to add notification");
        throw; // Re-throw to handle upstream
    }
}
    }
}
