namespace POD.DTO
{
    public class CustomProductResponseDTO
    {
        public int CustomProductId { get; set; }
        public string CustomName { get; set; }
        public string CustomDescription { get; set; }
        public string CustomImageUrl { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProductTemplateId { get; set; }
        public string UserId { get; set; }
    }
}
