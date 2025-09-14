using System.ComponentModel.DataAnnotations;

namespace VotingSystem_Claude.Models
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(100)]
        public string FullName { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginTime { get; set; }

        [StringLength(100)]
        public string? LastLoginIp { get; set; }

        public int FailedLoginAttempts { get; set; } = 0;

        public DateTime? LockoutEnd { get; set; }
    }
}