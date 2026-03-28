using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VotingSystem_Claude.Models
{
    public class Position
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = "";

        public string? Description { get; set; }

        public int MaxSelectableOptions { get; set; } = 1;

        public int DisplayOrder { get; set; }

        [ForeignKey("Election")]
        public int ElectionId { get; set; }

        // Navigation properties
        public virtual Election Election { get; set; } = null!;
        public virtual ICollection<Candidate> Candidates { get; set; } = [];
        public virtual ICollection<Vote> Votes { get; set; } = [];
    }
}
