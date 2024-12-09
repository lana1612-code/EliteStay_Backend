namespace Hotel_Backend_API.DTO.Complain
{
    public class ReturnComplainDTO
    {
        public int HotelId { get; set; }

        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
