using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using POD.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

[Route("api/[controller]")]
[ApiController]
public class ExternalAuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;

    public ExternalAuthController(UserManager<ApplicationUser> userManager, IConfiguration config)
    {
        _userManager = userManager;
        _config = config;
    }

    // Start external login (Google/Facebook/etc)
    [HttpGet("signin")]
    public IActionResult SignIn([FromQuery] string provider, [FromQuery] string returnUrl = "/")
    {
        var redirectUrl = Url.Action(nameof(Callback), "ExternalAuth", new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, provider);
    }

    // Callback called by external provider after authentication (browser lands here)
    [HttpGet("callback")]
    public async Task<IActionResult> Callback(string returnUrl = "/")
    {
        try
        {
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);

            if (!result.Succeeded || result.Principal == null)
                return Unauthorized(new { error = "External authentication failed or no principal found." });

            string provider = result.Properties.Items.TryGetValue(".AuthScheme", out var s) ? s : "Google";
            var providerKey = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

            // Find user by external login (best approach if user already attached to provider)
            var user = await _userManager.FindByLoginAsync(provider, providerKey);

            if (user == null)
            {
                var externalEmail = result.Principal.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(externalEmail))
                    return Unauthorized(new { error = "No email claim received from external provider." });

                user = await _userManager.FindByEmailAsync(externalEmail);

                if (user == null)
                {
                    var externalFirstName = result.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "";
                    var externalLastName = result.Principal.FindFirstValue(ClaimTypes.Surname) ?? "";
                    user = new ApplicationUser
                    {
                        UserName = externalEmail,
                        Email = externalEmail,
                        FirstName = externalFirstName,
                        LastName = externalLastName
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                        return BadRequest(new
                        {
                            error = "Failed to create user from external login.",
                            details = createResult.Errors.Select(e => e.Description)
                        });

                    await _userManager.AddToRoleAsync(user, "User");
                }

                // If provider login is not already linked, link it now
                var existingLogins = await _userManager.GetLoginsAsync(user);
                if (!existingLogins.Any(l => l.LoginProvider == provider && l.ProviderKey == providerKey))
                {
                    await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerKey, provider));
                }
            }

            // Generate JWT
            var userRoles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? ""),
                new(ClaimTypes.Email, user.Email ?? ""),
                new("FirstName", user.FirstName ?? ""),
                new("LastName", user.LastName ?? "")
            };
            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]!));
            var token = new JwtSecurityToken(
                issuer: _config["JWT:Issuer"],
                audience: _config["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(10),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Securely validate returnUrl to avoid open redirect
            var frontendUrl = _config["Frontend:BaseUrl"] ?? "http://localhost:4200";
            // (Optional) parse/append returnUrl if needed

            // Standard SPA: Redirect user with JWT in fragment for frontend to handle
            return Redirect($"{frontendUrl}#/auth/callback?token={jwt}");
        }
        catch (Exception ex)
        {
            // Aggregate error info
            var errorList = new List<string>();
            var curr = ex;
            while (curr != null)
            {
                errorList.Add(curr.Message);
                curr = curr.InnerException;
            }
            return StatusCode(500, new
            {
                error = "An unexpected error occurred during the external login callback.",
                messages = errorList,
                stackTrace = ex.StackTrace
            });
        }
    }
}
