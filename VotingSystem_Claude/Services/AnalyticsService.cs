using VotingSystem_Claude.Data;
using VotingSystem_Claude.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace VotingSystem_Claude.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(ApplicationDbContext context, ILogger<AnalyticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<VoterAnalytics> GetVoterAnalyticsAsync(int electionId)
        {
            try
            {
                var election = await _context.Elections
                    .Include(e => e.Positions)
                    .ThenInclude(p => p.Candidates)
                    .FirstOrDefaultAsync(e => e.Id == electionId);

                if (election == null)
                    throw new ArgumentException("Election not found");

                var votes = await _context.Votes
                    .Where(v => v.ElectionId == electionId)
                    .ToListAsync();

                var analytics = new VoterAnalytics
                {
                    TotalVoters = await _context.Voters.CountAsync(),
                    VotedCount = votes.Count,
                    TurnoutPercentage = votes.Count * 100.0 / await _context.Voters.CountAsync(),
                    VotesByPosition = new Dictionary<string, int>(),
                    VotesByCandidate = new Dictionary<string, int>(),
                    VotingTimeDistribution = new List<VotingTimeSlot>()
                };

                // Calculate votes by position
                foreach (var position in election.Positions)
                {
                    analytics.VotesByPosition[position.Title] = votes.Count(v => v.PositionId == position.Id);
                }

                // Calculate votes by candidate
                foreach (var position in election.Positions)
                {
                    foreach (var candidate in position.Candidates)
                    {
                        analytics.VotesByCandidate[candidate.FullName] = votes.Count(v => v.CandidateId == candidate.Id);
                    }
                }

                // Calculate voting time distribution
                var timeSlots = votes
                    .GroupBy(v => v.Timestamp.Date.AddHours(v.Timestamp.Hour))
                    .Select(g => new VotingTimeSlot
                    {
                        TimeSlot = g.Key,
                        VoteCount = g.Count()
                    })
                    .OrderBy(ts => ts.TimeSlot)
                    .ToList();

                analytics.VotingTimeDistribution = timeSlots;

                return analytics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting voter analytics for election {ElectionId}", electionId);
                throw;
            }
        }

        public async Task<VoterTurnoutStats> GetVoterTurnoutStatsAsync(int electionId)
        {
            try
            {
                var votes = await _context.Votes
                    .Where(v => v.ElectionId == electionId)
                    .ToListAsync();

                var stats = new VoterTurnoutStats
                {
                    TotalEligibleVoters = await _context.Voters.CountAsync(),
                    TotalVotesCast = votes.Count,
                    TurnoutPercentage = votes.Count * 100.0 / await _context.Voters.CountAsync(),
                    HourlyTurnout = new Dictionary<DateTime, int>(),
                    TurnoutByLocation = new Dictionary<string, int>()
                };

                // Calculate hourly turnout
                var hourlyVotes = votes
                    .GroupBy(v => v.Timestamp.Date.AddHours(v.Timestamp.Hour))
                    .ToDictionary(g => g.Key, g => g.Count());

                stats.HourlyTurnout = hourlyVotes;

                // Calculate turnout by location (if location data is available)
                var locationVotes = votes
                    .GroupBy(v => v.Location ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Count());

                stats.TurnoutByLocation = locationVotes;

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting voter turnout stats for election {ElectionId}", electionId);
                throw;
            }
        }

        public async Task<List<VoterDemographics>> GetVoterDemographicsAsync(int electionId)
        {
            try
            {
                var voters = await _context.Voters
                    .Include(v => v.Student)
                    .ToListAsync();

                var demographics = new List<VoterDemographics>();

                // Calculate age demographics
                var ageGroups = voters
                    .GroupBy(v => GetAgeGroup(v.Student?.DateOfBirth ?? DateTime.Now))
                    .Select(g => new VoterDemographics
                    {
                        Category = $"Age Group: {g.Key}",
                        Count = g.Count(),
                        Percentage = g.Count() * 100.0 / voters.Count
                    });

                demographics.AddRange(ageGroups);

                // Calculate gender demographics
                var genderGroups = voters
                    .GroupBy(v => v.Student?.Gender ?? "Unknown")
                    .Select(g => new VoterDemographics
                    {
                        Category = $"Gender: {g.Key}",
                        Count = g.Count(),
                        Percentage = g.Count() * 100.0 / voters.Count
                    });

                demographics.AddRange(genderGroups);

                return demographics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting voter demographics for election {ElectionId}", electionId);
                throw;
            }
        }

        public async Task<RealTimeStats> GetRealTimeStatsAsync(int electionId)
        {
            try
            {
                var oneHourAgo = DateTime.UtcNow.AddHours(-1);
                var votes = await _context.Votes
                    .Where(v => v.ElectionId == electionId)
                    .ToListAsync();

                var stats = new RealTimeStats
                {
                    ActiveVoters = votes.Count(v => v.Timestamp >= oneHourAgo),
                    VotesCastInLastHour = votes.Count(v => v.Timestamp >= oneHourAgo),
                    CurrentVotesByPosition = new Dictionary<string, int>(),
                    RecentActivity = new List<VotingActivity>()
                };

                // Calculate current votes by position
                var positions = await _context.Positions
                    .Where(p => p.ElectionId == electionId)
                    .ToListAsync();

                foreach (var position in positions)
                {
                    stats.CurrentVotesByPosition[position.Title] = votes.Count(v => v.PositionId == position.Id);
                }

                // Get recent voting activity
                stats.RecentActivity = votes
                    .OrderByDescending(v => v.Timestamp)
                    .Take(10)
                    .Select(v => new VotingActivity
                    {
                        Timestamp = v.Timestamp,
                        Position = positions.FirstOrDefault(p => p.Id == v.PositionId)?.Title ?? "Unknown",
                        Candidate = _context.Candidates.FirstOrDefault(c => c.Id == v.CandidateId)?.FullName ?? "Unknown",
                        Location = v.Location ?? "Unknown"
                    })
                    .ToList();

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting real-time stats for election {ElectionId}", electionId);
                throw;
            }
        }

        public async Task<VotingProgress> GetVotingProgressAsync(int electionId)
        {
            try
            {
                var election = await _context.Elections.FindAsync(electionId);
                if (election == null)
                    throw new ArgumentException("Election not found");

                var totalVoters = await _context.Voters.CountAsync();
                var votesCast = await _context.Votes.CountAsync(v => v.ElectionId == electionId);
                var timeRemaining = election.EndDate - DateTime.UtcNow;

                var progress = new VotingProgress
                {
                    TotalVotesCast = votesCast,
                    RemainingVotes = totalVoters - votesCast,
                    CompletionPercentage = votesCast * 100.0 / totalVoters,
                    EstimatedTimeRemaining = timeRemaining
                };

                return progress;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting voting progress for election {ElectionId}", electionId);
                throw;
            }
        }

        public async Task<byte[]> ExportResultsAsync(int electionId, string format)
        {
            try
            {
                var results = await GetVoterAnalyticsAsync(electionId);
                return format.ToLower() switch
                {
                    "csv" => GenerateCsvExport(results),
                    "pdf" => await GeneratePdfExportAsync(results),
                    "excel" => await GenerateExcelExportAsync(results),
                    _ => throw new ArgumentException("Unsupported export format")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting results for election {ElectionId} in format {Format}", electionId, format);
                throw;
            }
        }

        public async Task<byte[]> GenerateReportAsync(int electionId, string reportType)
        {
            try
            {
                var analytics = await GetVoterAnalyticsAsync(electionId);
                var turnout = await GetVoterTurnoutStatsAsync(electionId);
                var demographics = await GetVoterDemographicsAsync(electionId);

                var report = new
                {
                    ElectionId = electionId,
                    ReportType = reportType,
                    GeneratedAt = DateTime.UtcNow,
                    Analytics = analytics,
                    Turnout = turnout,
                    Demographics = demographics
                };

                return JsonSerializer.SerializeToUtf8Bytes(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report for election {ElectionId} of type {ReportType}", electionId, reportType);
                throw;
            }
        }

        public async Task<byte[]> GenerateCustomReportAsync(int electionId, ReportParameters parameters)
        {
            try
            {
                var report = new Dictionary<string, object>();

                if (parameters.IncludeMetrics.Contains("VoterAnalytics"))
                {
                    report["VoterAnalytics"] = await GetVoterAnalyticsAsync(electionId);
                }

                if (parameters.IncludeMetrics.Contains("TurnoutStats"))
                {
                    report["TurnoutStats"] = await GetVoterTurnoutStatsAsync(electionId);
                }

                if (parameters.IncludeMetrics.Contains("Demographics"))
                {
                    report["Demographics"] = await GetVoterDemographicsAsync(electionId);
                }

                return parameters.Format.ToLower() switch
                {
                    "json" => JsonSerializer.SerializeToUtf8Bytes(report),
                    "csv" => GenerateCsvExport(report),
                    "pdf" => await GeneratePdfExportAsync(report),
                    "excel" => await GenerateExcelExportAsync(report),
                    _ => throw new ArgumentException("Unsupported export format")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating custom report for election {ElectionId}", electionId);
                throw;
            }
        }

        public async Task<List<ReportTemplate>> GetAvailableReportTemplatesAsync()
        {
            return new List<ReportTemplate>
            {
                new ReportTemplate
                {
                    Name = "Election Summary",
                    Description = "Comprehensive summary of election results and statistics",
                    AvailableMetrics = new List<string> { "VoterAnalytics", "TurnoutStats", "Demographics" },
                    SupportedFormats = new List<string> { "PDF", "Excel", "CSV" }
                },
                new ReportTemplate
                {
                    Name = "Voter Turnout Analysis",
                    Description = "Detailed analysis of voter turnout and participation",
                    AvailableMetrics = new List<string> { "TurnoutStats", "Demographics" },
                    SupportedFormats = new List<string> { "PDF", "Excel" }
                },
                new ReportTemplate
                {
                    Name = "Candidate Performance",
                    Description = "Analysis of candidate performance and vote distribution",
                    AvailableMetrics = new List<string> { "VoterAnalytics" },
                    SupportedFormats = new List<string> { "PDF", "Excel", "CSV" }
                }
            };
        }

        public async Task<List<ElectionComparison>> CompareElectionsAsync(List<int> electionIds)
        {
            try
            {
                var comparisons = new List<ElectionComparison>();

                foreach (var electionId in electionIds)
                {
                    var election = await _context.Elections.FindAsync(electionId);
                    if (election == null) continue;

                    var analytics = await GetVoterAnalyticsAsync(electionId);
                    var turnout = await GetVoterTurnoutStatsAsync(electionId);

                    comparisons.Add(new ElectionComparison
                    {
                        ElectionId = electionId,
                        Title = election.Title,
                        TotalVoters = analytics.TotalVoters,
                        VotesCast = analytics.VotedCount,
                        TurnoutPercentage = analytics.TurnoutPercentage,
                        PositionComparison = analytics.VotesByPosition
                            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value * 100.0 / analytics.TotalVoters)
                    });
                }

                return comparisons;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing elections {ElectionIds}", string.Join(", ", electionIds));
                throw;
            }
        }

        public async Task<TrendAnalysis> GetVotingTrendsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var elections = await _context.Elections
                    .Where(e => e.StartDate >= startDate && e.EndDate <= endDate)
                    .OrderBy(e => e.StartDate)
                    .ToListAsync();

                var trendAnalysis = new TrendAnalysis
                {
                    TimePoints = new List<DateTime>(),
                    TurnoutTrend = new List<double>(),
                    ParticipationTrend = new List<double>(),
                    PositionTrends = new Dictionary<string, List<double>>()
                };

                foreach (var election in elections)
                {
                    var analytics = await GetVoterAnalyticsAsync(election.Id);
                    trendAnalysis.TimePoints.Add(election.StartDate);
                    trendAnalysis.TurnoutTrend.Add(analytics.TurnoutPercentage);
                    trendAnalysis.ParticipationTrend.Add(analytics.VotedCount * 100.0 / analytics.TotalVoters);

                    foreach (var position in analytics.VotesByPosition)
                    {
                        if (!trendAnalysis.PositionTrends.ContainsKey(position.Key))
                        {
                            trendAnalysis.PositionTrends[position.Key] = new List<double>();
                        }
                        trendAnalysis.PositionTrends[position.Key].Add(position.Value * 100.0 / analytics.TotalVoters);
                    }
                }

                return trendAnalysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting voting trends from {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        private string GetAgeGroup(DateTime dateOfBirth)
        {
            var age = DateTime.Now.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > DateTime.Now.AddYears(-age)) age--;

            return age switch
            {
                < 18 => "Under 18",
                < 25 => "18-24",
                < 35 => "25-34",
                < 45 => "35-44",
                < 55 => "45-54",
                < 65 => "55-64",
                _ => "65+"
            };
        }

        private byte[] GenerateCsvExport(object data)
        {
            // Implementation for CSV export
            throw new NotImplementedException();
        }

        private async Task<byte[]> GeneratePdfExportAsync(object data)
        {
            // Implementation for PDF export
            throw new NotImplementedException();
        }

        private async Task<byte[]> GenerateExcelExportAsync(object data)
        {
            // Implementation for Excel export
            throw new NotImplementedException();
        }
    }
} 