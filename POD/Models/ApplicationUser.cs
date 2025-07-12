using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;



namespace POD.Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual SellerProfile SellerProfile { get; set; }
        public virtual ICollection<CustomProduct> CustomProducts { get; set; } = new List<CustomProduct>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
