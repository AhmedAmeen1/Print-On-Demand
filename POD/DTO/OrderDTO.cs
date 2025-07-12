using System.ComponentModel.DataAnnotations;

namespace POD.DTO
{
    public class OrderDTO
    {
        [Required] public string ShippingAddress { get; set; }
        public List<OrderItemDTO> OrderItems { get; set; }
    }
}
