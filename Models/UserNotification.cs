using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_Backend_API.Models
{
    public class UserNotification
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        [ForeignKey(nameof(Notification))]
        public int NotificationId { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
        public Notification Notification { get; set; }
    }
}
