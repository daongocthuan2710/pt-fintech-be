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
            builder.Entity<User>().Property(u => u.RefreshToken).HasMaxLength(512);
            builder.Entity<User>().Property(u => u.RefreshTokenExpiryTime);

            builder.Entity<User>()
               .Property(u => u.Id)
               .HasMaxLength(36)
               .ValueGeneratedNever();

            builder.Entity<TaskItem>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
