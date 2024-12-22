namespace Hotel_Backend_API.DTO.Hotel
{
    public class AddHotel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int Stars { get; set; }
        public string profileIMG { get; set; }
        public string Tags { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
