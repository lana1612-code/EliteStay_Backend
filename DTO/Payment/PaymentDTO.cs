using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_Backend_API.DTO.Payment
{
    public class PaymentDTO
    {
        public int BookingId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }
        public string PaymentDate { get; set; }
        public string StatusDone { get; set; } 
    }
}
