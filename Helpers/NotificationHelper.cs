using LoanManagementSystem.Data;
using LoanManagementSystem.Models;

namespace LoanManagementSystem.Helpers
{
public static class NotificationHelper
{
    public static async Task AddNotificationAsync(ApplicationDbContext context, int userId, string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        var notification = new Notification
        {
            UserId = userId,
            Message = message,
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
            // Log error (you should inject a logger in a real application)
            Console.WriteLine($"Error adding notification: {ex.Message}");
        }
    }
}
}
