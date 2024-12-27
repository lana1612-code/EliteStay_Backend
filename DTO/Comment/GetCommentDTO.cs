namespace Hotel_Backend_API.DTO.Comment
{
    public class GetCommentDTO
    {
        public int Id { get; set; }
        public int HotelId { get; set; }
        public string username { get; set; }
        public string? userImg { get; set; }
        public string Content { get; set; }
        public string CreatedAt { get; set; } 
    }
}
