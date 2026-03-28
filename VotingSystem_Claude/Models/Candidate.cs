using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VotingSystem_Claude.Models
{
    public class Candidate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        [StringLength(50)]
        public string? Class { get; set; }

        public string? ImagePath { get; set; }

        public string? ManifestoSummary { get; set; }

        [ForeignKey("Position")]
        public int PositionId { get; set; }

        [ForeignKey("Student")]
        public int? StudentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Position Position { get; set; } = null!;
        public virtual Student? Student { get; set; }
        public virtual ICollection<Vote> Votes { get; set; } = [];
    }
}
