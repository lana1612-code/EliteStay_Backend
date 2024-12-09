using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_Backend_API.Models
{
    public class SavedRoom
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        [ForeignKey(nameof(Room))]
        public int RoomId { get; set; } 
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
        public virtual Room Room { get; set; }
    }
}
