namespace POD.DTO
{
    public class CartItemResponseDTO
    {
        public int CartItemId { get; set; }
        public int CustomProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; }
        public CustomProductResponseDTO CustomProduct { get; set; }
    }
}
