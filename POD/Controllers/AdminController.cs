using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POD.DTO;
using POD.Models;

namespace POD.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly Context _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(Context context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ---------------- USERS CRUD ----------------

        [HttpGet("Users")]
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetUsers()
        {
            return await _userManager.Users.ToListAsync();
        }

        [HttpGet("Users/{id}")]
        public async Task<ActionResult<ApplicationUser>> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return user;
        }

        [HttpDelete("Users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return NotFound();

                // Delete SellerProfiles
                var sellerProfiles = await _context.SellerProfiles
                    .Where(sp => sp.UserId == id)
                    .ToListAsync();
                if (sellerProfiles.Any())
                {
                    _context.SellerProfiles.RemoveRange(sellerProfiles);
                    await _context.SaveChangesAsync();
                }

                // Delete CartItems & OrderItems related to CustomProducts of the user
                var customProducts = await _context.CustomProducts
                    .Where(cp => cp.UserId == id)
                    .ToListAsync();

                foreach (var cp in customProducts)
                {
                    var cartItems = await _context.CartItems
                        .Where(ci => ci.CustomProductId == cp.CustomProductId)
                        .ToListAsync();
                    if (cartItems.Any())
                    {
                        _context.CartItems.RemoveRange(cartItems);
                    }

                    var orderItems = await _context.OrderItems
                        .Where(oi => oi.CustomProductId == cp.CustomProductId)
                        .ToListAsync();
                    if (orderItems.Any())
                    {
                        _context.OrderItems.RemoveRange(orderItems);
                    }
                }
                await _context.SaveChangesAsync();

                // Delete CustomProducts
                if (customProducts.Any())
                {
                    _context.CustomProducts.RemoveRange(customProducts);
                    await _context.SaveChangesAsync();
                }

                // Delete Orders and related Payments & OrderItems
                var orders = await _context.Orders
                    .Where(o => o.UserId == id)
                    .ToListAsync();

                foreach (var order in orders)
                {
                    var payments = await _context.Payments
                        .Where(p => p.OrderId == order.OrderId)
                        .ToListAsync();
                    if (payments.Any())
                    {
                        _context.Payments.RemoveRange(payments);
                    }

                    var orderItems = await _context.OrderItems
                        .Where(oi => oi.OrderId == order.OrderId)
                        .ToListAsync();
                    if (orderItems.Any())
                    {
                        _context.OrderItems.RemoveRange(orderItems);
                    }
                }
                await _context.SaveChangesAsync();

                if (orders.Any())
                {
                    _context.Orders.RemoveRange(orders);
                    await _context.SaveChangesAsync();
                }

                // Remove roles
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Any())
                {
                    var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, roles);
                    if (!removeRolesResult.Succeeded)
                        return BadRequest(removeRolesResult.Errors.Select(e => e.Description));
                }

                // Remove logins
                var logins = await _userManager.GetLoginsAsync(user);
                foreach (var login in logins)
                {
                    var removeLoginResult = await _userManager.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);
                    if (!removeLoginResult.Succeeded)
                        return BadRequest(removeLoginResult.Errors.Select(e => e.Description));
                }

                // Remove claims
                var claims = await _userManager.GetClaimsAsync(user);
                if (claims.Any())
                {
                    var removeClaimsResult = await _userManager.RemoveClaimsAsync(user, claims);
                    if (!removeClaimsResult.Succeeded)
                        return BadRequest(removeClaimsResult.Errors.Select(e => e.Description));
                }

                // Delete user
                var deleteResult = await _userManager.DeleteAsync(user);
                if (!deleteResult.Succeeded)
                {
                    return BadRequest(deleteResult.Errors.Select(e => e.Description));
                }

                return Ok("User and all related data deleted successfully");
            }
            catch (Exception ex)
            {
                var message = $"Error deleting user: {ex.Message}";
                if (ex.InnerException != null)
                    message += $" | Inner: {ex.InnerException.Message}";
                return StatusCode(500, message);
            }
        }



        [HttpPut("Users/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateDTO dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            user.UserName = dto.Email;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok("User updated successfully");
        }

        // ---------------- PRODUCT TEMPLATES CRUD ----------------

        [HttpGet("Templates")]
        public async Task<ActionResult<IEnumerable<ProductTemplate>>> GetAllTemplates()
        {
            return await _context.ProductTemplates.ToListAsync();
        }

        [HttpGet("Templates/{id}")]
        public async Task<ActionResult<ProductTemplate>> GetTemplate(int id)
        {
            var template = await _context.ProductTemplates.FindAsync(id);
            if (template == null) return NotFound();
            return template;
        }

        [HttpPost("Templates")]
        public async Task<ActionResult<ProductTemplate>> CreateTemplate([FromBody] ProductTemplateDTO dto)
        {
            var template = new ProductTemplate
            {
                Name = dto.Name,
                Description = dto.Description,
                BasePrice = dto.BasePrice,
                Category = dto.Category,
                ImageUrl = dto.ImageUrl,
                Elements = dto.Elements,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                SellerProfileId = 5  // Assign to default seller
            };

            _context.ProductTemplates.Add(template);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTemplate), new { id = template.ProductTemplateId }, template);
        }


        [HttpPut("Templates/{id}")]
        public async Task<IActionResult> UpdateTemplate(int id, [FromBody] ProductTemplateDTO dto)
        {
            var template = await _context.ProductTemplates.FindAsync(id);
            if (template == null) return NotFound();

            template.Name = dto.Name;
            template.Description = dto.Description;
            template.BasePrice = dto.BasePrice;
            template.Category = dto.Category;
            template.ImageUrl = dto.ImageUrl;
            template.Elements = dto.Elements;

            await _context.SaveChangesAsync();
            return Ok("Template updated successfully");
        }

        [HttpDelete("Templates/{id}")]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            var template = await _context.ProductTemplates.FindAsync(id);
            if (template == null) return NotFound();

            _context.ProductTemplates.Remove(template);
            await _context.SaveChangesAsync();

            return Ok("Template deleted successfully");
        }
    }

    // DTOs
    public class UserUpdateDTO
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
    }
}
