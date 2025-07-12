using POD.Models;

namespace POD.DTO
{
    public class PaymentResponseDTO
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public PaymentMethod Method { get; set; }
        public DateTime PaymentDate { get; set; }
        public string TransactionId { get; set; }
    }
}
