using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagement_BE.models;

namespace TaskManagement_BE.data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TaskItem> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>()
                .Property(u => u.PasswordHash)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Entity<IdentityRole>()
                .Property(r => r.Name)
                .HasMaxLength(256)
                .IsRequired();

            builder.Entity<IdentityUserRole<string>>()
                .HasKey(r => new { r.UserId, r.RoleId });

            builder.Entity<User>()
                .HasMany<IdentityUserRole<string>>()
                .WithOne()
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            builder.Entity<IdentityRole>()
                .HasMany<IdentityUserRole<string>>()
                .WithOne()
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
        }
    }
}
