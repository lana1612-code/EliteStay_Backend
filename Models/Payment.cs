using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_Backend_API.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [ForeignKey("Booking")]
        public int BookingId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }
        public string StatusDone { get; set; } = "NO";

        [MaxLength(10)]
        public string Method { get; set; } = "Cash";

        public virtual Booking Booking { get; set; }
    }

    //  public enum PaymentMethod
    //  {
    //      Cash,
    //      BankCard
    //  }
}
