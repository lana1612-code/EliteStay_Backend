namespace Hotel_Backend_API.DTO.Complain
{
    public class ReturnComplainDTO
    {
        public int Id { get; set; }
        public string NameUser { get; set; }
        public string hotelName { get; set; }
        public string EmailUser { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
