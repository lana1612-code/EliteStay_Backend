namespace Hotel_Backend_API.DTO.Booking
{
    public class BookingDTO
    {
        public int Id { get; set; }
        public string GuestName { get; set; }
        public string RoomNumber { get; set; }
        public string CheckinDate { get; set; }
        public string CheckoutDate { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
