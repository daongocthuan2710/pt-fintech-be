using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskManagement_BE.models;
using TaskManagement_BE.DTOs;
using TaskManagement_BE.Repositories;
using Microsoft.AspNetCore.Identity;

namespace TaskManagement_BE.Services
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string username, string password);
        Task<string> GenerateAccessTokenAsync(User user);
        string GenerateRefreshToken();
        Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken);

        Task<IdentityResult> RegisterAsync(RegisterUserDto registerDto);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository userRepository, IConfiguration config)
        {
            _userRepository = userRepository;
            _config = config;
        }

        public async Task<IdentityResult> RegisterAsync(RegisterUserDto registerDto)
        {
            // Confirm password
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Passwords do not match." });
            }

            // Check existed email
            if (await _userRepository.GetUserByEmailAsync(registerDto.Email) != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Email is already registered." });
            }

            // Create new user
            var user = new User
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                EmailConfirmed = true
            };

            return await _userRepository.CreateUserAsync(user, registerDto.Password);
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);
            if (user == null)
                throw new UnauthorizedAccessException("Invalid username or password.");

            if (!await _userRepository.ValidatePasswordAsync(user, password))
                throw new UnauthorizedAccessException("Invalid username or password.");

            return await GenerateAccessTokenAsync(user);
        }

        public async Task<string> GenerateAccessTokenAsync(User user)
        {
            var roles = new List<string> { "user" };
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

            var secretKey = _config["JwtSettings:SecretKey"];
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

        public async Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userRepository.GetUserByUsernameAsync(refreshToken);
            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            var newAccessToken = await GenerateAccessTokenAsync(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateUserAsync(user);

            return (newAccessToken, newRefreshToken);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
