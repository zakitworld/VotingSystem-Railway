using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VotingSystem_Claude.Models
{
    public class Vote
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Voter")]
        public int VoterId { get; set; }

        [ForeignKey("Candidate")]
        public int CandidateId { get; set; }

        [ForeignKey("Position")]
        public int PositionId { get; set; }

        [ForeignKey("Election")]
        public int ElectionId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? Location { get; set; }

        // Navigation properties
        public virtual Voter Voter { get; set; }
        public virtual Candidate Candidate { get; set; }
        public virtual Position Position { get; set; }
        public virtual Election Election { get; set; }
    }
}
