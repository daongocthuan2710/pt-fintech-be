using Microsoft.AspNetCore.Identity;
using TaskManagement_BE.models;
using Microsoft.EntityFrameworkCore;
using TaskManagement_BE.data;
using TaskManagement_BE.utils;
using System.Text.Json;

namespace TaskManagement_BE.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IdentityResult> CreateUserAsync(User user, string password);
        Task UpdateUserAsync(User user);
        Task<bool> ValidatePasswordAsync(User user, string password);
        Task<bool> AddUserToRoleAsync(User user, string roleName);
        Task<bool> RoleExistsAsync(string roleName);
        Task<IdentityRole> CreateRoleAsync(string roleName);
        Task<bool> IsUserInRoleAsync(User user, string roleName);
    }

    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public UserRepository(UserManager<User> userManager, AppDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
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

        public async Task<bool> AddUserToRoleAsync(User user, string roleName)
        {
            await _userManager.AddToRoleAsync(user, roleName);
            return true;
        }

        public async Task<bool> RoleExistsAsync(string roleName)
        {
            Console.WriteLine($"✅ Debug: roleName is {roleName}");
            return await _roleManager.RoleExistsAsync(roleName);
        }

        public async Task<IdentityRole> CreateRoleAsync(string roleName)
        {
            var role = new IdentityRole(roleName);
            await _roleManager.CreateAsync(role);
            return role;
        }

        public async Task<bool> IsUserInRoleAsync(User user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }
    }
}
