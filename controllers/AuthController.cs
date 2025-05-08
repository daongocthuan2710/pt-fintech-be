using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

using TaskManagement_BE.models;

namespace TaskManagement_BE.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;

        public AuthController(UserManager<User> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login model)
        {
            if (model == null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
                return BadRequest("Username and Password are required.");

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
                return Unauthorized("Invalid username or password.");

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized("Invalid username or password.");

            // Generate JWT Token
            var token = await GenerateAccessTokenAsync(user);
            return Ok(new { AccessToken = token });
        }

        private async Task<string> GenerateAccessTokenAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? ""),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email ?? "")
    };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            string secretKey = _config["JwtSettings:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new Exception("JWT SecretKey is not configured. Please check appsettings.json.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest("Invalid Refresh Token.");

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return Unauthorized("Invalid or expired refresh token.");

            var newAccessToken = await GenerateAccessTokenAsync(user);
            var newRefreshToken = GenerateRefreshToken();

            // Cập nhật Refresh Token mới cho User
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

    }

}
