using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskManagement_BE.data;
using TaskManagement_BE.models;
using TaskManagement_BE.utils;

namespace TaskManagement_BE.data
{
    public static class SeedData
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var context = services.GetRequiredService<AppDbContext>();

            await context.Database.MigrateAsync();

            // Make sure the database has applied the migrations
            await context.Database.MigrateAsync();

            // Seed Roles
            await SeedRolesAsync(roleManager);

            // Seed Admin User
            await SeedAdminUserAsync(userManager, roleManager);

            // Seed Normal Users and Tasks
            // await SeedUsersAndTasksAsync(userManager, roleManager, context);
        }

        // Seed roles method (admin and user)
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { "admin", "user" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }


        // Admin seed method
        private static async Task SeedAdminUserAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            var admin = await userManager.Users.FirstOrDefaultAsync(u => u.UserName == "admin");
            if (admin == null)
            {
                admin = new User { Id = GuidUtil.GenerateGuid(), UserName = "admin", Email = "admin@example.com", EmailConfirmed = true };
                var result = await userManager.CreateAsync(admin, "Admin@123");
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
                await userManager.AddToRoleAsync(admin, "admin");
            }

            if (!await roleManager.RoleExistsAsync("admin"))
                await roleManager.CreateAsync(new IdentityRole("admin"));

            if (!await userManager.IsInRoleAsync(admin, "admin"))
                await userManager.AddToRoleAsync(admin, "admin");
        }

        // Method of seeding users and tasks
        private static async Task SeedUsersAndTasksAsync(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context)
        {
            const int numOfUsers = 3;
            for (int i = 1; i <= numOfUsers; i++)
            {
                var userName = $"user{i}";
                var userEmail = $"user{i}@example.com";
                var user = await userManager.Users.FirstOrDefaultAsync(u => u.UserName == userName);
                string validateUser = JsonSerializer.Serialize(user, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                Console.WriteLine(validateUser);

                if (user == null)
                {
                    user = new User { Id = GuidUtil.GenerateGuid(), UserName = userName, NormalizedUserName = userName.ToUpper(), Email = userEmail, EmailConfirmed = true };

                    if (string.IsNullOrWhiteSpace(user.UserName))
                    {
                        Console.WriteLine("UserName cannot be empty.");
                        break;
                    }

                    var result = await userManager.CreateAsync(user, "User@123");
                    string jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                    Console.WriteLine(jsonResult);

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
                        break;
                    }
                }

                if (!await roleManager.RoleExistsAsync("user"))
                    await roleManager.CreateAsync(new IdentityRole("user"));

                if (!await userManager.IsInRoleAsync(user, "user"))
                    await userManager.AddToRoleAsync(user, "user");

                // await SeedTasksForUserAsync(context, user);
            }
        }

        // Method of seeding tasks in batch
        private static async Task SeedTasksForUserAsync(AppDbContext context, User user)
        {
            if (await context.Tasks.AnyAsync(t => t.UserId == user.Id))
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
            string jsonUser = JsonSerializer.Serialize(user, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            Console.WriteLine(jsonUser);
            Console.WriteLine("✅ Debug: Tasks");
            string jsonTasks = JsonSerializer.Serialize(tasks, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            Console.WriteLine(jsonTasks);

            await context.Tasks.AddRangeAsync(tasks);
            await context.SaveChangesAsync();
        }

    }
}
