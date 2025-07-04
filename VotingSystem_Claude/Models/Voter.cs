using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VotingSystem_Claude.Models
{
    public class Voter
    {
        [Key]
        public int Id { get; set; }

        public bool HasVoted { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }

        public DateTime? LastLoginTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Student Student { get; set; }
        public virtual ICollection<Vote> Votes { get; set; }
        public virtual VoterCode VoterCode { get; set; }
    }
}
