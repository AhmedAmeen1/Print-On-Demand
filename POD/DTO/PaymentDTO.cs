using POD.Models;
using System.ComponentModel.DataAnnotations;

namespace POD.DTO
{
    public class PaymentDTO
    {
        [Required] public decimal Amount { get; set; }
        [Required] public PaymentMethod Method { get; set; }
        public string TransactionId { get; set; }
    }
}
