using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_Backend_API.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [ForeignKey("Guest")]
        public int GuestId { get; set; }

        [ForeignKey("Room")]
        public int RoomId { get; set; }
        public DateTime CheckinDate { get; set; }
        public DateTime CheckoutDate { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        public virtual Guest Guest { get; set; }
        public virtual Room Room { get; set; } // to allow lazy loading 
        public virtual Payment Payment { get; set; }// [ 1 - 1 ] Paymen - Booking
    }
}
