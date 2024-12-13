using System.ComponentModel.DataAnnotations;

namespace Hotel_Backend_API.DTO.Rating
{
    public class AddRatingDto
    {
        public int HotelId { get; set; }
        [Range(1.0, 5.0, ErrorMessage = "Rating must be between 1 and 5.")]
        public double RatingValue { get; set; }
    }
}
