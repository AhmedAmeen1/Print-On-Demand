using POD.Models;

namespace POD.DTO
{
    public class ProductTemplateResponseDTO
    {
        public int ProductTemplateId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public ProductCategory Category { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int SellerProfileId { get; set; }
        public string? Elements { get; set; }

    }
}
