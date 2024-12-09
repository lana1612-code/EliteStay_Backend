using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_Backend_API.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        [ForeignKey(nameof(Hotel))]
        public int HotelId { get; set; } 
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual Hotel Hotel { get; set; }
    }
}
