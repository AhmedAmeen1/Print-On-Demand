using POD.Models;
using System.ComponentModel.DataAnnotations;

namespace POD.DTO
{
    public class ProductTemplateDTO
    {
        [Required] public string Name { get; set; }
        public string Description { get; set; }
        [Required] public decimal BasePrice { get; set; }
        [Required] public ProductCategory Category { get; set; }
        public string ImageUrl { get; set; }
        public string? Elements { get; set; }
    }
}
