namespace Hotel_Backend_API.DTO.Notification
{
    public class NotificationDTO
    {
        public string Message { get; set; }
        public string sentAt { get; set; }
        public bool IsRead { get; set; }
    }
}
