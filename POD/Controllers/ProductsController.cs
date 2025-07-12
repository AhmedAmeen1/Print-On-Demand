using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POD.DTO;
using POD.Models;
using System.Security.Claims;

namespace POD.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly Context _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductsController(Context context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Products/Templates (for sellers)
        [HttpGet("Templates")]
        [Authorize(Roles = "Seller")]
        public async Task<ActionResult<IEnumerable<ProductTemplateResponseDTO>>> GetProductTemplates()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sellerProfile = await _context.SellerProfiles
                .Include(sp => sp.ProductTemplates)
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (sellerProfile == null) return NotFound("Seller profile not found");

            return sellerProfile.ProductTemplates.Select(pt => new ProductTemplateResponseDTO
            {
                ProductTemplateId = pt.ProductTemplateId,
                Name = pt.Name,
                Description = pt.Description,
                BasePrice = pt.BasePrice,
                Category = pt.Category,
                ImageUrl = pt.ImageUrl,
                IsActive = pt.IsActive,
                CreatedAt = pt.CreatedAt,
                SellerProfileId = pt.SellerProfileId
            }).ToList();
        }


        // GET: api/Products/PublicTemplates
        [HttpGet("PublicTemplates")]
        [AllowAnonymous] // Or [Authorize] if you want only logged-in users
        public async Task<ActionResult<IEnumerable<ProductTemplateResponseDTO>>> GetPublicProductTemplates()
        {
            var templates = await _context.ProductTemplates
                .Where(pt => pt.IsActive) // show only active templates
                .ToListAsync();

            return templates.Select(pt => MapToTemplateResponse(pt)).ToList();
        }





        // POST: api/Products/Templates (for sellers)
        [HttpPost("Templates")]
        [Authorize(Roles = "Seller")]
        public async Task<ActionResult<ProductTemplateResponseDTO>> CreateProductTemplate(ProductTemplateDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sellerProfile = await _context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (sellerProfile == null) return BadRequest("User is not a seller");

            var template = new ProductTemplate
            {
                Name = dto.Name,
                Description = dto.Description,
                BasePrice = dto.BasePrice,
                Category = dto.Category,
                ImageUrl = dto.ImageUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                SellerProfileId = sellerProfile.SellerProfileId
            };

            _context.ProductTemplates.Add(template);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProductTemplate),
                new { id = template.ProductTemplateId },
                MapToTemplateResponse(template));
        }

        // GET: api/Products/Templates/{id}
        [HttpGet("Templates/{id}")]
        public async Task<ActionResult<ProductTemplateResponseDTO>> GetProductTemplate(int id)
        {
            var template = await _context.ProductTemplates.FindAsync(id);
            if (template == null) return NotFound();
            return MapToTemplateResponse(template);
        }

        // POST: api/Products/Custom (for users)
        [HttpPost("Custom")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<CustomProductResponseDTO>> CreateCustomProduct(CustomProductDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var template = await _context.ProductTemplates.FindAsync(dto.ProductTemplateId);
            if (template == null) return BadRequest("Invalid product template");

            var customProduct = new CustomProduct
            {
                UserId = userId,
                ProductTemplateId = dto.ProductTemplateId,
                CustomName = dto.CustomName,
                CustomDescription = dto.CustomDescription,
                CustomImageUrl = dto.CustomImageUrl,
                Price = dto.Price,
                CreatedAt = DateTime.UtcNow
            };

            _context.CustomProducts.Add(customProduct);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomProduct),
                new { id = customProduct.CustomProductId },
                MapToCustomProductResponse(customProduct));
        }

        // GET: api/Products/Custom/{id}
        [HttpGet("Custom/{id}")]
        public async Task<ActionResult<CustomProductResponseDTO>> GetCustomProduct(int id)
        {
            var customProduct = await _context.CustomProducts
                .Include(cp => cp.ProductTemplate)
                .FirstOrDefaultAsync(cp => cp.CustomProductId == id);

            if (customProduct == null) return NotFound();
            return MapToCustomProductResponse(customProduct);
        }

        // Helper methods
        private ProductTemplateResponseDTO MapToTemplateResponse(ProductTemplate template)
        {
            return new ProductTemplateResponseDTO
            {
                ProductTemplateId = template.ProductTemplateId,
                Name = template.Name,
                Description = template.Description,
                BasePrice = template.BasePrice,
                Category = template.Category,
                ImageUrl = template.ImageUrl,
                IsActive = template.IsActive,
                CreatedAt = template.CreatedAt,
                SellerProfileId = template.SellerProfileId
            };
        }

        private CustomProductResponseDTO MapToCustomProductResponse(CustomProduct customProduct)
        {
            return new CustomProductResponseDTO
            {
                CustomProductId = customProduct.CustomProductId,
                CustomName = customProduct.CustomName,
                CustomDescription = customProduct.CustomDescription,
                CustomImageUrl = customProduct.CustomImageUrl,
                Price = customProduct.Price,
                CreatedAt = customProduct.CreatedAt,
                ProductTemplateId = customProduct.ProductTemplateId,
                UserId = customProduct.UserId
            };
        }
    }
}
