using System.ComponentModel.DataAnnotations;

namespace POD.DTO
{
    public class SellerProfileDTO
    {
        [Required] public string StoreName { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
    }
}
