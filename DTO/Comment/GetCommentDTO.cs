namespace Hotel_Backend_API.DTO.Comment
{
    public class GetCommentDTO
    {
        public int HotelId { get; set; }
        public string Content { get; set; }
        public string CreatedAt { get; set; } 
    }
}
