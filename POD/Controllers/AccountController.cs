using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using POD.DTO;
using POD.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace POD.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly Context _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            Context context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.Role != "User" && model.Role != "Seller")
                return BadRequest("Invalid role. Must be 'User' or 'Seller'");

            if (model.Role == "Seller" && string.IsNullOrEmpty(model.StoreName))
                return BadRequest("StoreName is required for Sellers");

            var user = new ApplicationUser
            {
                UserName = model.Email, // Use email as username for consistency
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await EnsureRolesExist();
                await _userManager.AddToRoleAsync(user, model.Role);

                if (model.Role == "Seller")
                {
                    var sellerProfile = new SellerProfile
                    {
                        UserId = user.Id,
                        StoreName = model.StoreName,
                        Description = model.Description ?? "",
                        Address = model.Address ?? "",
                        PhoneNumber = model.PhoneNumber ?? "",
                        IsVerified = true,
                        CreatedAt = DateTime.Now
                    };

                    _context.SellerProfiles.Add(sellerProfile);
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    message = "User registered successfully",
                    role = model.Role,
                    userId = user.Id
                });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = model.Email?.Trim().ToLower();
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Email is required");

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpper());

            if (user == null)
                return Unauthorized("User not found");

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized("Invalid password");

            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Email, user.Email),
                new("FirstName", user.FirstName ?? ""),
                new("LastName", user.LastName ?? "")
            };

            foreach (var role in userRoles)
                authClaims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: authClaims,
                expires: DateTime.UtcNow.AddHours(10),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            // Only load seller profile if user is a Seller
            SellerProfileResponseDTO? sellerProfileDto = null;
            if (userRoles.Contains("Seller"))
            {
                var sellerProfile = await _context.SellerProfiles
                    .FirstOrDefaultAsync(sp => sp.UserId == user.Id);

                if (sellerProfile != null)
                {
                    sellerProfileDto = new SellerProfileResponseDTO
                    {
                        SellerProfileId = sellerProfile.SellerProfileId,
                        StoreName = sellerProfile.StoreName,
                        Description = sellerProfile.Description,
                        Address = sellerProfile.Address,
                        PhoneNumber = sellerProfile.PhoneNumber,
                        IsVerified = sellerProfile.IsVerified,
                        CreatedAt = sellerProfile.CreatedAt,
                        UserId = sellerProfile.UserId
                    };
                }
            }

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expires_at = token.ValidTo,
                user = new UserResponseDTO
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = userRoles.FirstOrDefault(),
                    SellerProfile = sellerProfileDto
                }
            });
        }

        private async Task EnsureRolesExist()
        {
            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole("User"));

            if (!await _roleManager.RoleExistsAsync("Seller"))
                await _roleManager.CreateAsync(new IdentityRole("Seller"));
        }
    }
}
