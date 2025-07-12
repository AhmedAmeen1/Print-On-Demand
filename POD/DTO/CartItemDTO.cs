using System.ComponentModel.DataAnnotations;

namespace POD.DTO
{
    public class CartItemDTO
    {
        [Required] public int CustomProductId { get; set; }
        [Required] public int Quantity { get; set; }
    }
}
