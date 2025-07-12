using System.ComponentModel.DataAnnotations;

namespace POD.DTO
{

    public class OrderItemDTO
    {
        [Required] public int CustomProductId { get; set; }
        [Required] public int Quantity { get; set; }
        [Required] public decimal UnitPrice { get; set; }
    }
}
