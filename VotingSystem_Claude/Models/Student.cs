using System.ComponentModel.DataAnnotations;

namespace VotingSystem_Claude.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(20)]
        public string StudentId { get; set; }

        [StringLength(50)]
        public string Class { get; set; }

        [StringLength(20)]
        public string House { get; set; }

        public string Gender { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Voter Voter { get; set; }
        public virtual ICollection<Candidate> Candidates { get; set; }
    }
}
