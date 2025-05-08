using Microsoft.AspNetCore.Identity;
using TaskManagement_BE.models;
using Microsoft.EntityFrameworkCore;
using TaskManagement_BE.data;
using TaskManagement_BE.utils;
using System.Text.Json;
using TaskManagement_BE.Constants;

namespace TaskManagement_BE.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IdentityResult> CreateUserAsync(User user, string password, string? role);
        Task UpdateUserAsync(User user);
        Task<bool> ValidatePasswordAsync(string userName, string password);
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

        private readonly SignInManager<User> _signInManager;

        private readonly PasswordHasher<User> _passwordHasher = new();

        public UserRepository(UserManager<User> userManager, AppDbContext context, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IdentityResult> CreateUserAsync(User user, string password, string? role = ROLE.User)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    user.Id = GuidUtil.GenerateGuid();
                    Console.WriteLine($"password = {password}");
                    user.PasswordHash = _passwordHasher.HashPassword(user, password);
                    _context.Users.Add(user);

                    // var jsonResult = JsonSerializer.Serialize(resultTest, new JsonSerializerOptions { WriteIndented = true });
                    // Console.WriteLine(jsonResult);
                    var result = await _userManager.CreateAsync(user, password);

                    if (!result.Succeeded)
                    {
                        Console.WriteLine("User creation failed.");
                        await transaction.RollbackAsync();
                        return result;
                    }
                    await _context.SaveChangesAsync();

                    var roleExists = await _roleManager.RoleExistsAsync(role);
                    if (!roleExists)
                    {
                        await this.CreateRoleAsync(role);
                    }

                    roleExists = await _roleManager.RoleExistsAsync(role);
                    if (!roleExists)
                    {
                        Console.WriteLine("Role creation failed.");
                        await transaction.RollbackAsync();
                        return IdentityResult.Failed(new IdentityError { Description = "Role creation failed." });
                    }

                    await _context.SaveChangesAsync();

                    if (!string.IsNullOrEmpty(role) && roleExists)
                    {
                        var createdUser = await _userManager.FindByIdAsync(user.Id.ToString());
                        if (createdUser == null)
                        {
                            Console.WriteLine("Debug: User not found after creation.");
                            await transaction.RollbackAsync();
                            return IdentityResult.Failed(new IdentityError { Description = "User creation failed." });
                        }

                        var userTest = new User
                        {
                            Id = createdUser.Id,
                            UserName = createdUser.UserName,
                            Email = createdUser.Email,
                        };
                        Console.WriteLine("Debug: userTest");
                        var jsonCreatedUser = JsonSerializer.Serialize(userTest, new JsonSerializerOptions { WriteIndented = true });
                        Console.WriteLine(userTest);
                        var addRoleResult = await this.AddUserToRoleAsync(userTest, role);
                        if (!addRoleResult)
                        {
                            Console.WriteLine("Failed to add role to user.");
                            await transaction.RollbackAsync();
                            return IdentityResult.Failed(new IdentityError { Description = "Failed to add role to user." });
                        }

                        Console.WriteLine($"Debug: Role {role} added to user.");
                    }

                    await transaction.CommitAsync();
                    return result;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Error: {ex.Message}");
                    return IdentityResult.Failed(new IdentityError { Description = ex.Message });
                }
            }

        }

        public async Task UpdateUserAsync(User user)
        {
            await _userManager.UpdateAsync(user);
        }

        public async Task<bool> ValidatePasswordAsync(string username, string password)
        {
            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserName == username);

                if (user == null)
                {
                    Console.WriteLine("User not found.");
                    return false;
                }
                var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
                bool isValid = Convert.ToBoolean(result);
                return isValid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ValidatePasswordAsync: {ex.Message}");
                return false;
            }
        }



        public async Task<bool> AddUserToRoleAsync(User user, string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                role = new IdentityRole(roleName);
                var createRoleResult = await _roleManager.CreateAsync(role);
                if (!createRoleResult.Succeeded)
                {
                    Console.WriteLine($"Failed to create role {roleName}. Errors:");
                    foreach (var error in createRoleResult.Errors)
                    {
                        Console.WriteLine($" - {error.Code}: {error.Description}");
                    }
                    return false;
                }
                Console.WriteLine($"Role {roleName} created.");
            }

            var userHasRole = await _userManager.IsInRoleAsync(user, roleName);
            if (userHasRole)
            {
                Console.WriteLine($"User {user.UserName} already has role {roleName}.");
                return true;
            }

            // var addRoleResult = await _userManager.AddToRoleAsync(user, roleName);
            // if (!addRoleResult.Succeeded)
            // {
            //     Console.WriteLine($"Failed to add role {roleName} to user {user.UserName}. Errors:");
            //     foreach (var error in addRoleResult.Errors)
            //     {
            //         Console.WriteLine($" - {error.Code}: {error.Description}");
            //     }
            //     return false;
            // }

            Console.WriteLine($"User {user.UserName} added to role {roleName}.");
            return true;
        }


        public async Task<bool> RoleExistsAsync(string roleName)
        {
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
