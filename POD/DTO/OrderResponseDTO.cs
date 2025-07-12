using POD.Models;

namespace POD.DTO
{

    public class OrderResponseDTO
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public string ShippingAddress { get; set; }
        public List<OrderItemResponseDTO> OrderItems { get; set; }
        public List<PaymentResponseDTO> Payments { get; set; }
    }
}
