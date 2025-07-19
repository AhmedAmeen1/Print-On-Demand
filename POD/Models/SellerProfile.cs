using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POD.Models
{
    public class SellerProfile
    {
        [Key]
        public int SellerProfileId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string StoreName { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        public bool IsVerified { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<ProductTemplate> ProductTemplates { get; set; } = new List<ProductTemplate>();
    }
}
