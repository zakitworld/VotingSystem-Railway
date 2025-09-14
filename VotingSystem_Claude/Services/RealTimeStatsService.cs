using Microsoft.EntityFrameworkCore;
using VotingSystem_Claude.Data;
using VotingSystem_Claude.Models;
using VotingSystem_Claude.Services.Interfaces;

namespace VotingSystem_Claude.Services
{
    public class RealTimeStatsService : IRealTimeStatsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RealTimeStatsService> _logger;
        private Timer? _statsTimer;
        private int _currentElectionId;

        public event EventHandler<VotingStatistics>? StatisticsUpdated;

        public RealTimeStatsService(ApplicationDbContext context, ILogger<RealTimeStatsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<VotingStatistics> GetElectionStatisticsAsync(int electionId)
        {
            try
            {
                var election = await _context.Elections
                    .Include(e => e.Positions)
                        .ThenInclude(p => p.Candidates)
                    .Include(e => e.Votes)
                        .ThenInclude(v => v.Candidate)
                    .FirstOrDefaultAsync(e => e.Id == electionId);

                if (election == null)
                {
                    return new VotingStatistics { ElectionId = electionId };
                }

                var totalVoters = await _context.Voters.CountAsync();
                var votesCount = election.Votes.Select(v => v.VoterId).Distinct().Count();

                var stats = new VotingStatistics
                {
                    ElectionId = electionId,
                    ElectionTitle = election.Title,
                    TotalVoters = totalVoters,
                    VotesCount = votesCount,
                    LastUpdated = DateTime.UtcNow
                };

                foreach (var position in election.Positions)
                {
                    var positionVotes = election.Votes.Where(v => v.PositionId == position.Id).ToList();
                    var positionStats = new PositionStatistics
                    {
                        PositionId = position.Id,
                        PositionTitle = position.Title,
                        TotalVotes = positionVotes.Count
                    };

                    foreach (var candidate in position.Candidates)
                    {
                        var candidateVotes = positionVotes.Count(v => v.CandidateId == candidate.Id);
                        var percentage = positionVotes.Count > 0 ? (candidateVotes / (double)positionVotes.Count) * 100 : 0;

                        positionStats.CandidateStats.Add(new CandidateStatistics
                        {
                            CandidateId = candidate.Id,
                            CandidateName = candidate.FullName,
                            CandidateClass = candidate.Class ?? "",
                            VoteCount = candidateVotes,
                            VotePercentage = percentage
                        });
                    }

                    // Sort by vote count descending
                    positionStats.CandidateStats = positionStats.CandidateStats
                        .OrderByDescending(c => c.VoteCount)
                        .ToList();

                    stats.PositionStats.Add(positionStats);
                }

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting election statistics for election {ElectionId}", electionId);
                return new VotingStatistics { ElectionId = electionId };
            }
        }

        public async Task<Dictionary<string, int>> GetHourlyVotingTrendsAsync(int electionId)
        {
            try
            {
                var votes = await _context.Votes
                    .Where(v => v.ElectionId == electionId)
                    .GroupBy(v => new { Hour = v.Timestamp.Hour })
                    .Select(g => new { Hour = g.Key.Hour, Count = g.Count() })
                    .OrderBy(x => x.Hour)
                    .ToListAsync();

                var trends = new Dictionary<string, int>();
                for (int hour = 0; hour < 24; hour++)
                {
                    var hourStr = $"{hour:00}:00";
                    trends[hourStr] = votes.FirstOrDefault(v => v.Hour == hour)?.Count ?? 0;
                }

                return trends;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hourly voting trends for election {ElectionId}", electionId);
                return new Dictionary<string, int>();
            }
        }

        public async Task StartMonitoringAsync(int electionId)
        {
            _currentElectionId = electionId;
            
            // Update immediately
            var stats = await GetElectionStatisticsAsync(electionId);
            StatisticsUpdated?.Invoke(this, stats);

            // Start periodic updates every 30 seconds
            _statsTimer = new Timer(async _ =>
            {
                try
                {
                    var updatedStats = await GetElectionStatisticsAsync(_currentElectionId);
                    StatisticsUpdated?.Invoke(this, updatedStats);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in periodic statistics update");
                }
            }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }

        public Task StopMonitoringAsync()
        {
            _statsTimer?.Dispose();
            _statsTimer = null;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _statsTimer?.Dispose();
        }
    }
}