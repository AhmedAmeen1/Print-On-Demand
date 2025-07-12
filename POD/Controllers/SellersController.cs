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
    [Authorize(Roles = "Seller")]
    public class SellersController : ControllerBase
    {
        private readonly Context _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SellersController(Context context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Sellers/Profile
        [HttpGet("Profile")]
        public async Task<ActionResult<SellerProfileResponseDTO>> GetSellerProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (profile == null) return NotFound("Seller profile not found");
            return MapToProfileResponse(profile);
        }

        // PUT: api/Sellers/Profile
        [HttpPut("Profile")]
        public async Task<IActionResult> UpdateSellerProfile(SellerProfileDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (profile == null) return NotFound("Seller profile not found");

            profile.StoreName = dto.StoreName;
            profile.Description = dto.Description;
            profile.Address = dto.Address;
            profile.PhoneNumber = dto.PhoneNumber;

            _context.SellerProfiles.Update(profile);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Sellers/Products
        [HttpGet("Products")]
        public async Task<ActionResult<IEnumerable<ProductTemplateResponseDTO>>> GetSellerProducts()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _context.SellerProfiles
                .Include(sp => sp.ProductTemplates)
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            if (profile == null) return NotFound("Seller profile not found");

            return profile.ProductTemplates.Select(MapToTemplateResponse).ToList();
        }

        // Helper methods
        private SellerProfileResponseDTO MapToProfileResponse(SellerProfile profile)
        {
            return new SellerProfileResponseDTO
            {
                SellerProfileId = profile.SellerProfileId,
                StoreName = profile.StoreName,
                Description = profile.Description,
                Address = profile.Address,
                PhoneNumber = profile.PhoneNumber,
                IsVerified = profile.IsVerified,
                CreatedAt = profile.CreatedAt,
                UserId = profile.UserId
            };
        }

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
    }
}
