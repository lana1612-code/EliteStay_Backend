using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Hotel_Backend_API.DTO.Payment
{
    public class UpdatePaymentDTO
    {

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }
        public string PaymentDate { get; set; }
    }
}
