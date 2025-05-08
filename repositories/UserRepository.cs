using Microsoft.AspNetCore.Identity;
using TaskManagement_BE.models;
using Microsoft.EntityFrameworkCore;
using TaskManagement_BE.data;
using TaskManagement_BE.utils;
using System.Text.Json;

namespace TaskManagement_BE.Repositories
{
    private readonly ILogger<AuthService> _logger;
    public interface IUserRepository
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IdentityResult> CreateUserAsync(User user, string password);
        Task UpdateUserAsync(User user);
        Task<bool> ValidatePasswordAsync(User user, string password);
    }

    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;

        public UserRepository(UserManager<User> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IdentityResult> CreateUserAsync(User user, string password)
        {
            user.Id = GuidUtil.GenerateGuid();
            string jsonUser = JsonSerializer.Serialize(user, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            _logger.LogInformation("Debug: User - {User}", jsonUser);
            return await _userManager.CreateAsync(user, password);
        }

        public async Task UpdateUserAsync(User user)
        {
            await _userManager.UpdateAsync(user);
        }

        public async Task<bool> ValidatePasswordAsync(User user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }
    }
}
