using VotingSystem_Claude.Data;
using VotingSystem_Claude.Services.Interfaces;
using VotingSystem_Claude.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using FluentAssertions;
using VotingSystem_Claude.Models;
using Microsoft.Extensions.Logging;

namespace VotingSystem_Claude.Tests.Services
{
    public class AnalyticsServiceTests
    {
        private readonly Mock<ILogger<AnalyticsService>> _loggerMock;
        private readonly ApplicationDbContext _context;
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _loggerMock = new Mock<ILogger<AnalyticsService>>();
            _analyticsService = new AnalyticsService(_context, _loggerMock.Object);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Add test election
            var election = new Election
            {
                Id = 1,
                Title = "Test Election",
                Description = "Test Description",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(1),
                IsActive = true
            };
            _context.Elections.Add(election);

            // Add test position
            var position = new Position
            {
                Id = 1,
                ElectionId = 1,
                Title = "Test Position",
                DisplayOrder = 1
            };
            _context.Positions.Add(position);

            // Add test candidates
            var candidate1 = new Candidate
            {
                Id = 1,
                PositionId = 1,
                FullName = "Test Candidate 1"
            };
            var candidate2 = new Candidate
            {
                Id = 2,
                PositionId = 1,
                FullName = "Test Candidate 2"
            };
            _context.Candidates.AddRange(candidate1, candidate2);

            // Add test voters
            var voters = new List<Voter>
            {
                new Voter { Id = 1, StudentId = 1 },
                new Voter { Id = 2, StudentId = 2 },
                new Voter { Id = 3, StudentId = 3 }
            };
            _context.Voters.AddRange(voters);

            // Add test votes
            var votes = new List<Vote>
            {
                new Vote
                {
                    Id = 1,
                    ElectionId = 1,
                    PositionId = 1,
                    CandidateId = 1,
                    VoterId = 1,
                    Timestamp = DateTime.UtcNow.AddMinutes(-30) // Within last hour
                },
                new Vote
                {
                    Id = 2,
                    ElectionId = 1,
                    PositionId = 1,
                    CandidateId = 2,
                    VoterId = 2,
                    Timestamp = DateTime.UtcNow.AddMinutes(-10) // Within last hour
                }
            };
            _context.Votes.AddRange(votes);

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetVoterAnalyticsAsync_ShouldReturnCorrectAnalytics()
        {
            // Act
            var result = await _analyticsService.GetVoterAnalyticsAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.TotalVoters.Should().Be(3);
            result.VotedCount.Should().Be(2);
            result.TurnoutPercentage.Should().BeApproximately(66.67, 0.01);
            result.VotesByPosition.Should().ContainKey("Test Position");
            result.VotesByPosition["Test Position"].Should().Be(2);
            result.VotesByCandidate.Should().ContainKey("Test Candidate 1");
            result.VotesByCandidate.Should().ContainKey("Test Candidate 2");
        }

        [Fact]
        public async Task GetVoterTurnoutStatsAsync_ShouldReturnCorrectStats()
        {
            // Act
            var result = await _analyticsService.GetVoterTurnoutStatsAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.TotalEligibleVoters.Should().Be(3);
            result.TotalVotesCast.Should().Be(2);
            result.TurnoutPercentage.Should().BeApproximately(66.67, 0.01);
        }

        [Fact]
        public async Task GetRealTimeStatsAsync_ShouldReturnCorrectStats()
        {
            // Act
            var result = await _analyticsService.GetRealTimeStatsAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.ActiveVoters.Should().Be(2); // Both votes are within last hour
            result.VotesCastInLastHour.Should().Be(2);
        }

        [Fact]
        public async Task GetVotingProgressAsync_ShouldReturnCorrectProgress()
        {
            // Act
            var result = await _analyticsService.GetVotingProgressAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.TotalVotesCast.Should().Be(2);
            result.RemainingVotes.Should().Be(1);
            result.CompletionPercentage.Should().BeApproximately(66.67, 0.01);
        }

        [Fact]
        public async Task GetAvailableReportTemplatesAsync_ShouldReturnTemplates()
        {
            // Act
            var result = await _analyticsService.GetAvailableReportTemplatesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
        }
    }
}