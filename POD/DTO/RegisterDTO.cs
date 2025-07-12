using System.ComponentModel.DataAnnotations;

namespace POD.DTO
{
    public class RegisterDTO
    {
        [Required]
        public string Username { get; set; }


        [Required, EmailAddress]
        public string Email { get; set; }


        [Required]
        public string Password { get; set; }
        
        
        [MaxLength(100)]
        public string FirstName { get; set; }
       
        
        [MaxLength(100)]
        public string LastName { get; set; }
        
        
        [Required]
        public string Role { get; set; } // "User" or "Seller"
        public string? StoreName { get; set; } // For sellers
        public string? Description { get; set; } // For sellers
        public string? Address { get; set; } // For sellers
        public string? PhoneNumber { get; set; } // For sellers
    }
}
