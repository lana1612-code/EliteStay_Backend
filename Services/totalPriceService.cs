namespace Hotel_Backend_API.Services
{
    public class totalPriceService
    {
        public decimal CalculateTotalPrice(DateTime checkinDate, DateTime checkoutDate, decimal pricePerNight)
        {
            int numberOfNights = (checkoutDate - checkinDate).Days;
            decimal totalPrice = numberOfNights  * pricePerNight;

            return totalPrice;
        }

    }
}
