using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement_BE.models
{
    public class User : IdentityUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}