using VotingSystem_Claude.Models;

namespace VotingSystem_Claude.Services.Interfaces
{
    public interface IRealTimeStatsService
    {
        Task<VotingStatistics> GetElectionStatisticsAsync(int electionId);
        Task<Dictionary<string, int>> GetHourlyVotingTrendsAsync(int electionId);
        event EventHandler<VotingStatistics> StatisticsUpdated;
        Task StartMonitoringAsync(int electionId);
        Task StopMonitoringAsync();
    }
}