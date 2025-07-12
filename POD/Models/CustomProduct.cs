using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POD.Models
{
    public class CustomProduct
    {
        [Key]
        public int CustomProductId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int ProductTemplateId { get; set; }

        [Required]
        [MaxLength(200)]
        public string CustomName { get; set; }

        [MaxLength(1000)]
        public string CustomDescription { get; set; }

        [MaxLength(500)]
        public string CustomImageUrl { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("ProductTemplateId")]
        public virtual ProductTemplate ProductTemplate { get; set; }

        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
