namespace Hotel_Backend_API.Models
{
    public class Like
    {
        public int Id { get; set; }
        public string UserId { get; set; } 
        public int RoomId { get; set; }
        public virtual Room Room { get; set; }
    }
}
