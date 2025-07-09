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

            context.Notifications.Add(notification);
            await context.SaveChangesAsync();
        }
    }
}
