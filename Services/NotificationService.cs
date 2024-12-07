using Hotel_Backend_API.Data;
using Hotel_Backend_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Backend_API.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _dbContext;

        public NotificationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task CreateNotificationAsync(string userId, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _dbContext.Notifications.Add(notification);
            await _dbContext.SaveChangesAsync();

            var userNotification = new UserNotification
            {
                UserId = userId,
                NotificationId = notification.Id,
                IsRead = false,
                SentAt = DateTime.UtcNow
            };

            _dbContext.UserNotifications.Add(userNotification);
            await _dbContext.SaveChangesAsync();
        }


        public async Task<List<UserNotification>> GetUserNotificationsAsync(string userId)
        {
            return await _dbContext.UserNotifications
                .Include(un => un.Notification)
                .Where(un => un.UserId == userId)
                .OrderByDescending(un => un.SentAt)
                .ToListAsync();
        }


        public async Task MarkAsReadAsync(int userNotificationId)
        {
            var userNotification = await _dbContext.UserNotifications.FindAsync(userNotificationId);
            if (userNotification != null)
            {
                userNotification.IsRead = true;
                await _dbContext.SaveChangesAsync();
            }
        }
    }

}
