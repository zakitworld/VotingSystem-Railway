using Microsoft.EntityFrameworkCore;
using Moq;
using VotingSystem_Claude.Data;
using VotingSystem_Claude.Services;
using VotingSystem_Claude.Services.Interfaces;
using Xunit;
using FluentAssertions;
using VotingSystem_Claude.Models;
using Microsoft.Extensions.Logging;

namespace VotingSystem_Claude.Tests.Services
{
    public class ElectionServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly IElectionService _electionService;

        public ElectionServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _electionService = new ElectionService(_context);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Add test elections
            var elections = new List<Election>
            {
                new Election
                {
                    Title = "Test Election 1",
                    Description = "Test Description 1",
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(1),
                    IsActive = true
                },
                new Election
                {
                    Title = "Test Election 2",
                    Description = "Test Description 2",
                    StartDate = DateTime.UtcNow.AddDays(-2),
                    EndDate = DateTime.UtcNow.AddDays(-1),
                    IsActive = false
                }
            };
            _context.Elections.AddRange(elections);
            _context.SaveChanges();

            // Add test positions
            var e1 = _context.Elections.First(e => e.Title == "Test Election 1");
            var positions = new List<Position>
            {
                new Position
                {
                    ElectionId = e1.Id,
                    Title = "Test Position 1",
                    DisplayOrder = 1
                },
                new Position
                {
                    ElectionId = e1.Id,
                    Title = "Test Position 2",
                    DisplayOrder = 2
                }
            };
            _context.Positions.AddRange(positions);
            _context.SaveChanges();

            // Add test candidates
            var p1 = _context.Positions.First(p => p.Title == "Test Position 1");
            var p2 = _context.Positions.First(p => p.Title == "Test Position 2");
            var candidates = new List<Candidate>
            {
                new Candidate
                {
                    PositionId = p1.Id,
                    FullName = "Test Candidate 1"
                },
                new Candidate
                {
                    PositionId = p1.Id,
                    FullName = "Test Candidate 2"
                },
                new Candidate
                {
                    PositionId = p2.Id,
                    FullName = "Test Candidate 3"
                }
            };
            _context.Candidates.AddRange(candidates);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetElectionByIdAsync_ValidId_ShouldReturnElection()
        {
            // Arrange
            var election = await _context.Elections.FirstAsync(e => e.Title == "Test Election 1");

            // Act
            var result = await _electionService.GetElectionByIdAsync(election.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(election.Id);
            result.Title.Should().Be("Test Election 1");
            result.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task GetElectionByIdAsync_InvalidId_ShouldReturnNull()
        {
            // Act
            var result = await _electionService.GetElectionByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllElectionsAsync_ShouldReturnAllElections()
        {
            // Act
            var result = await _electionService.GetAllElectionsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(e => e.Title == "Test Election 1");
            result.Should().Contain(e => e.Title == "Test Election 2");
        }

        [Fact]
        public async Task GetActiveElectionAsync_ShouldReturnActiveElection()
        {
            // Act
            var result = await _electionService.GetActiveElectionAsync();

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("Test Election 1");
            result.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task CreateElectionAsync_ValidElection_ShouldSucceed()
        {
            // Arrange
            var election = new Election
            {
                Title = "New Election",
                Description = "New Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                IsActive = true
            };

            // Act
            var result = await _electionService.CreateElectionAsync(election);

            // Assert
            result.Should().NotBeNull();
            var savedElection = await _context.Elections
                .FirstOrDefaultAsync(e => e.Title == "New Election");
            savedElection.Should().NotBeNull();
            savedElection!.Description.Should().Be("New Description");
        }

        [Fact]
        public async Task UpdateElectionAsync_ValidUpdate_ShouldSucceed()
        {
            // Arrange
            var election = await _context.Elections.FirstAsync(e => e.Title == "Test Election 1");
            election.Title = "Updated Election";
            election.Description = "Updated Description";

            // Act
            var result = await _electionService.UpdateElectionAsync(election);

            // Assert
            result.Should().BeTrue();
            var updatedElection = await _context.Elections
                .FirstOrDefaultAsync(e => e.Id == election.Id);
            updatedElection.Should().NotBeNull();
            updatedElection!.Title.Should().Be("Updated Election");
            updatedElection.Description.Should().Be("Updated Description");
        }

        [Fact]
        public async Task DeleteElectionAsync_ValidElection_ShouldSucceed()
        {
            // Arrange
            var election = await _context.Elections.FirstAsync(e => e.Title == "Test Election 1");

            // Act
            var result = await _electionService.DeleteElectionAsync(election.Id);

            // Assert
            result.Should().BeTrue();
            var deletedElection = await _context.Elections
                .FirstOrDefaultAsync(e => e.Id == election.Id);
            deletedElection.Should().BeNull();
        }
    }
}