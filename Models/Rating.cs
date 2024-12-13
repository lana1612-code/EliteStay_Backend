namespace Hotel_Backend_API.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public string UserId { get; set; } 
        public int HotelId { get; set; }
        public double RatingValue { get; set; }
        public DateTime RatedAt { get; set; } = DateTime.UtcNow;

        public virtual Hotel Hotel { get; set; }
    }
}
