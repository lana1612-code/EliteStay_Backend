using System.ComponentModel.DataAnnotations;

namespace Hotel_Backend_API.Models
{
    public class Complaint
    {
        public int Id { get; set; } 

        public string UserId { get; set; } 
        public int HotelId { get; set; }  

        [Required]
        public string Content { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;   

        public virtual Hotel Hotel { get; set; }
    }
}
