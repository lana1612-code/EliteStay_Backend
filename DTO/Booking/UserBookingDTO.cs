namespace Hotel_Backend_API.DTO.Booking
{
    public class UserBookingDTO
    {
        public int Id { get; set; }
        public string GuestName { get; set; }
        public string GuestImg { get; set; }
        public string hotelName { get; set; }
        public string RoomName { get; set; }
        public string? ImageURL { get; set; }
        public string RoomNumber { get; set; }
        public string CheckinDate { get; set; }
        public string CheckoutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsEnd { get; set; }
    }
}
