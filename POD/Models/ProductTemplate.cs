using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace POD.Models
{

    public class ProductTemplate
    {
        [Key]
        public int ProductTemplateId { get; set; }

        [Required]
        public int SellerProfileId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; }

        public string? Elements { get; set; }

        public ProductCategory Category { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("SellerProfileId")]
        public virtual SellerProfile SellerProfile { get; set; }

        public virtual ICollection<CustomProduct> CustomProducts { get; set; } = new List<CustomProduct>();
    }

    public enum ProductCategory
    {
        TShirt,
        Pants,
        Hoodie,
        Mug,
        PhoneCase,
    }
}
