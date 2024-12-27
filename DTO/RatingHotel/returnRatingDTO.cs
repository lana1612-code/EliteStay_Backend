namespace Hotel_Backend_API.DTO.Rating
{
    public class returnRatingDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string? userImg { get; set; }
        public string hotelName { get; set; }
        public double RatingValue { get; set; }
    }
}
