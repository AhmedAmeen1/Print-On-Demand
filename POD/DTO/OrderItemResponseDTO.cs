namespace POD.DTO
{

    public class OrderItemResponseDTO
    {
        public int OrderItemId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public CustomProductResponseDTO CustomProduct { get; set; }
    }
}
