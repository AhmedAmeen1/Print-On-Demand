using System.ComponentModel.DataAnnotations;

namespace POD.DTO
{

    public class CustomProductDTO
    {
        [Required] public int ProductTemplateId { get; set; }
        [Required] public string CustomName { get; set; }
        public string CustomDescription { get; set; }
        public string CustomImageUrl { get; set; }
        [Required] public decimal Price { get; set; }
    }
}
