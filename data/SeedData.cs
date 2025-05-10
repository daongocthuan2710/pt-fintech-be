using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskManagement_BE.data;
using TaskManagement_BE.models;
using TaskManagement_BE.utils;
using TaskManagement_BE.Repositories;
using TaskManagement_BE.Constants;

namespace TaskManagement_BE.data
{
    public static class SeedData
    {
        private static IUserRepository _userRepository;
        private static RoleManager<IdentityRole> _roleManager;
        private static AppDbContext _context;

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            _userRepository = services.GetRequiredService<IUserRepository>();
            _roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            _context = services.GetRequiredService<AppDbContext>();

            Console.WriteLine("Applying migrations...");
            await _context.Database.MigrateAsync();
            Console.WriteLine("Migrations applied.");

            Console.WriteLine("Seeding data...");
            await SeedRolesAsync();
            await SeedAdminUserAsync();
            await SeedUsersAndTasksAsync();
            Console.WriteLine("Data seeded successfully.");
        }

        private static async Task SeedRolesAsync()
        {
            var roles = new[] { ROLE.Admin, ROLE.User };
            foreach (var role in roles)
            {
                var isExisted = await _userRepository.RoleExistsAsync(role);
                if (!isExisted)
                {
                    var result = await _userRepository.CreateRoleAsync(role);
                }
            }
        }

        private static async Task SeedAdminUserAsync()
        {
            string adminUserName = "admin";
            string adminPassword = "Admin@123";
            var admin = await _userRepository.GetUserByUsernameAsync(adminUserName);
            if (admin == null)
            {
                admin = new User
                {
                    UserName = adminUserName,
                    Email = "admin@example.com",
                    EmailConfirmed = true
                };
                var result = await _userRepository.CreateUserAsync(admin, adminPassword, ROLE.Admin);
                if (result.Succeeded)
                {
                    Console.WriteLine($"Admin created successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to create admin. Errors:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($" - {error.Code}: {error.Description}");
                    }
                }
            }
        }

        private static async Task SeedUsersAndTasksAsync()
        {
            const int numOfUsers = 3;
            for (int i = 1; i <= numOfUsers; i++)
            {
                var userName = $"user{i}";
                var userEmail = $"user{i}@example.com";
                var user = await _userRepository.GetUserByUsernameAsync(userName);

                if (user == null)
                {
                    user = new User
                    {
                        UserName = userName,
                        Email = userEmail,
                        EmailConfirmed = true
                    };

                    var result = await _userRepository.CreateUserAsync(user, "User@123", ROLE.User);
                    if (result.Succeeded)
                    {
                        Console.WriteLine($"User {userName} created successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to create user {userName}. Errors:");
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($" - {error.Code}: {error.Description}");
                        }
                        continue;
                    }
                }

                user = await _userRepository.GetUserByUsernameAsync(userName);
                await SeedTasksForUserAsync(user);
            }
        }

        private static async Task SeedTasksForUserAsync(User user)
        {
            if (await _context.Tasks.AnyAsync(t => t.UserId == user.Id))
                return;

            var tasks = new List<TaskItem>();

            for (int j = 1; j <= 10; j++)
            {
                tasks.Add(new TaskItem
                {
                    Title = $"Task {j} for {user.UserName}",
                    Description = $"Description for task {j}",
                    Status = "to-do",
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(j).ToUniversalTime().Subtract(DateTime.UnixEpoch).TotalMilliseconds,
                    UserId = user.Id
                });
            }
            await _context.Tasks.AddRangeAsync(tasks);
            await _context.SaveChangesAsync();
        }
    }
}
