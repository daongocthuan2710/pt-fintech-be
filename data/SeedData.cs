using Microsoft.AspNetCore.Identity;
using TaskManagement_BE.data;
using TaskManagement_BE.models;

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

            await context.Database.EnsureCreatedAsync();

            // Seed Roles
            if (!await roleManager.RoleExistsAsync("admin"))
                await roleManager.CreateAsync(new IdentityRole("admin"));
            if (!await roleManager.RoleExistsAsync("user"))
                await roleManager.CreateAsync(new IdentityRole("user"));

            // Seed Admin User
            var admin = await userManager.FindByNameAsync("admin");
            if (admin == null)
            {
                admin = new User { UserName = "admin", Email = "admin@example.com" };
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, "admin");
            }

            // Seed Normal Users and Tasks
            for (int i = 1; i <= 3; i++)
            {
                var userName = $"user{i}";
                var userEmail = $"user{i}@example.com";
                var user = await userManager.FindByNameAsync(userName);

                if (user == null)
                {
                    user = new User { UserName = userName, Email = userEmail };
                    await userManager.CreateAsync(user, "User@123");
                    await userManager.AddToRoleAsync(user, "user");
                }

                // Seed 10 Tasks for each user
                if (!context.Tasks.Any(t => t.UserId == user.Id))
                {
                    var tasks = new List<TaskItem>();
                    for (int j = 1; j <= 10; j++)
                    {
                        tasks.Add(new TaskItem
                        {
                            Title = $"Task {j} for {user.UserName}",
                            Description = $"Description for task {j}",
                            Status = "To-Do",
                            CreateAt = DateTime.UtcNow,
                            UpdateAt = DateTime.UtcNow,
                            DueDate = DateTime.UtcNow.AddDays(j),
                            UserId = user.Id
                        });
                    }

                    context.Tasks.AddRange(tasks);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
