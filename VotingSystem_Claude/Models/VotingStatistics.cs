namespace VotingSystem_Claude.Models
{
    public class VotingStatistics
    {
        public int ElectionId { get; set; }
        public string ElectionTitle { get; set; } = string.Empty;
        public int TotalVoters { get; set; }
        public int VotesCount { get; set; }
        public double TurnoutPercentage => TotalVoters > 0 ? (VotesCount / (double)TotalVoters) * 100 : 0;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public List<PositionStatistics> PositionStats { get; set; } = new();
    }

    public class PositionStatistics
    {
        public int PositionId { get; set; }
        public string PositionTitle { get; set; } = string.Empty;
        public int TotalVotes { get; set; }
        public List<CandidateStatistics> CandidateStats { get; set; } = new();
    }

    public class CandidateStatistics
    {
        public int CandidateId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateClass { get; set; } = string.Empty;
        public int VoteCount { get; set; }
        public double VotePercentage { get; set; }
    }
}