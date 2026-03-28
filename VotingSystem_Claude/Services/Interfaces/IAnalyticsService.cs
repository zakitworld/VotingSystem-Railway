using VotingSystem_Claude.Models;

namespace VotingSystem_Claude.Services.Interfaces
{
    public interface IAnalyticsService
    {
        // Voter Analytics
        Task<VoterAnalytics> GetVoterAnalyticsAsync(int electionId);
        Task<VoterTurnoutStats> GetVoterTurnoutStatsAsync(int electionId);
        Task<List<VoterDemographics>> GetVoterDemographicsAsync(int electionId);

        // Real-time Statistics
        Task<RealTimeStats> GetRealTimeStatsAsync(int electionId);
        Task<VotingProgress> GetVotingProgressAsync(int electionId);

        // Export Capabilities
        Task<byte[]> ExportResultsAsync(int electionId, string format);
        Task<byte[]> GenerateReportAsync(int electionId, string reportType);

        // Custom Reports
        Task<byte[]> GenerateCustomReportAsync(int electionId, ReportParameters parameters);
        Task<List<ReportTemplate>> GetAvailableReportTemplatesAsync();

        // Historical Analysis
        Task<List<ElectionComparison>> CompareElectionsAsync(List<int> electionIds);
        Task<TrendAnalysis> GetVotingTrendsAsync(DateTime startDate, DateTime endDate);
    }

    public class VoterAnalytics
    {
        public int TotalVoters { get; set; }
        public int VotedCount { get; set; }
        public double TurnoutPercentage { get; set; }
        public Dictionary<string, int> VotesByPosition { get; set; } = [];
        public Dictionary<string, int> VotesByCandidate { get; set; } = [];
        public List<VotingTimeSlot> VotingTimeDistribution { get; set; } = [];
    }

    public class VoterTurnoutStats
    {
        public int TotalEligibleVoters { get; set; }
        public int TotalVotesCast { get; set; }
        public double TurnoutPercentage { get; set; }
        public Dictionary<DateTime, int> HourlyTurnout { get; set; } = [];
        public Dictionary<string, int> TurnoutByLocation { get; set; } = [];
    }

    public class VoterDemographics
    {
        public string Category { get; set; } = null!;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class RealTimeStats
    {
        public int ActiveVoters { get; set; }
        public int VotesCastInLastHour { get; set; }
        public Dictionary<string, int> CurrentVotesByPosition { get; set; } = [];
        public List<VotingActivity> RecentActivity { get; set; } = [];
    }

    public class VotingProgress
    {
        public int TotalVotesCast { get; set; }
        public int RemainingVotes { get; set; }
        public double CompletionPercentage { get; set; }
        public TimeSpan EstimatedTimeRemaining { get; set; }
    }

    public class ReportParameters
    {
        public string ReportType { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> IncludeMetrics { get; set; } = [];
        public string Format { get; set; } = null!;
    }

    public class ReportTemplate
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public List<string> AvailableMetrics { get; set; } = [];
        public List<string> SupportedFormats { get; set; } = [];
    }

    public class ElectionComparison
    {
        public int ElectionId { get; set; }
        public string Title { get; set; } = null!;
        public int TotalVoters { get; set; }
        public int VotesCast { get; set; }
        public double TurnoutPercentage { get; set; }
        public Dictionary<string, double> PositionComparison { get; set; } = [];
    }

    public class TrendAnalysis
    {
        public List<DateTime> TimePoints { get; set; } = [];
        public List<double> TurnoutTrend { get; set; } = [];
        public List<double> ParticipationTrend { get; set; } = [];
        public Dictionary<string, List<double>> PositionTrends { get; set; } = [];
    }

    public class VotingTimeSlot
    {
        public DateTime TimeSlot { get; set; }
        public int VoteCount { get; set; }
    }

    public class VotingActivity
    {
        public DateTime Timestamp { get; set; }
        public string Position { get; set; } = null!;
        public string Candidate { get; set; } = null!;
        public string? Location { get; set; }
    }
}
