using System.ComponentModel.DataAnnotations;

namespace VotingSystem_Claude.Models
{
    public class Election
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }

        public string Description { get; set; }

        public string SchoolYear { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Position> Positions { get; set; }
    }
}
