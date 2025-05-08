using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskManagement_BE.data;
using TaskManagement_BE.models;
using TaskManagement_BE.utils;
using TaskManagement_BE.Repositories;

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

            await _context.Database.MigrateAsync();
            await SeedRolesAsync();
            await SeedAdminUserAsync();
            // await SeedUsersAndTasksAsync();
        }

        private static async Task SeedRolesAsync()
        {
            var roles = new[] { "admin", "user" };
            foreach (var role in roles)
            {
                var isExisted = await _userRepository.RoleExistsAsync(role);
                Console.WriteLine($"✅ Debug: isExisted is {isExisted}");
                if (!isExisted)
                {
                    var result = await _userRepository.CreateRoleAsync(role);
                    // if (result.Succeeded)
                    // {
                    //     Console.WriteLine($"Role {role} created successfully.");
                    // }
                    // else
                    // {
                    //     Console.WriteLine($"Failed to create role {role}. Errors:");
                    //     foreach (var error in result.Errors)
                    //     {
                    //         Console.WriteLine($" - {error.Code}: {error.Description}");
                    //     }
                    // }
                }
            }
        }

        private static async Task SeedAdminUserAsync()
        {
            string adminUserName = "admin";
            string adminPassword = "Admin@123";
            var admin = await _userRepository.GetUserByUsernameAsync(adminUserName);
            Console.WriteLine("✅ Debug: admin");
            var jsonAdmin = JsonSerializer.Serialize(admin, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(jsonAdmin);
            if (admin == null)
            {
                admin = new User
                {
                    UserName = adminUserName,
                    Email = "admin@example.com",
                    EmailConfirmed = true
                };
                var result = await _userRepository.CreateUserAsync(admin, adminPassword);
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

                var adminInfo = await _userRepository.GetUserByUsernameAsync(adminUserName);
                Console.WriteLine("✅ Debug: adminInfo");
                var jsonAdminInfo = JsonSerializer.Serialize(adminInfo, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(adminInfo);
                // await _userRepository.AddUserToRoleAsync(admin, "admin");
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
                        Id = GuidUtil.GenerateGuid(),
                        UserName = userName,
                        Email = userEmail,
                        EmailConfirmed = true
                    };

                    var result = await _userRepository.CreateUserAsync(user, "User@123");
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

                await _userRepository.AddUserToRoleAsync(user, "user");
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
                    DueDate = DateTime.UtcNow.AddDays(j),
                    UserId = user.Id
                });
            }

            Console.WriteLine("✅ Debug: User");
            string jsonUser = JsonSerializer.Serialize(user, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(jsonUser);
            Console.WriteLine("✅ Debug: Tasks");
            string jsonTasks = JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(jsonTasks);

            await _context.Tasks.AddRangeAsync(tasks);
            await _context.SaveChangesAsync();
        }
    }
}
