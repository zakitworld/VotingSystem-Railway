using System.ComponentModel.DataAnnotations;

namespace VotingSystem_Claude.Models
{
    public class VoterCode
    {
        public int Id { get; set; }
        
        [Required]
        public string Code { get; set; }
        
        public bool IsUsed { get; set; }
        
        public DateTime GeneratedAt { get; set; }
        
        public DateTime? UsedAt { get; set; }
        
        public int VoterId { get; set; }
        public Voter Voter { get; set; }
    }
} 