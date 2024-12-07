namespace Hotel_Backend_API.DTO.Booking
{
    public class UpdateBookingDTO
    {
        public int GuestId { get; set; }
        public int RoomId { get; set; }
        public string CheckinDate { get; set; }
        public string CheckoutDate { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
