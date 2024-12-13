namespace Hotel_Backend_API.Models
{
    public class Recommended
    {
        public int Id { get; set; }
        public int HotelId { get; set; }
        public double RatingValue { get; set; }
    }
}
