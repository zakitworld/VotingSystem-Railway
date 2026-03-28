using System.ComponentModel.DataAnnotations;

namespace VotingSystem_Claude.Models
{
    public class CandidateModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string? Class { get; set; }
        public int PositionId { get; set; }
        public int? StudentId { get; set; }
        public string? ImagePath { get; set; }
        public string? ManifestoSummary { get; set; }
    }
}
