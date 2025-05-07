using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagement_BE.models;

namespace TaskManagement_BE.data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<TaskItem> Tasks { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Đảm bảo rằng bảng TaskItem có Primary Key
            modelBuilder.Entity<TaskItem>().HasKey(t => t.Id);
        }
    }
}
