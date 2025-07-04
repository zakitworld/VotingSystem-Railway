using Microsoft.EntityFrameworkCore;
using Moq;
using VotingSystem_Claude.Data;
using VotingSystem_Claude.Services;
using VotingSystem_Claude.Services.Interfaces;
using Xunit;
using FluentAssertions;

namespace VotingSystem_Claude.Tests.Services
{
    public class ElectionServiceTests
    {
        private readonly Mock<ILogger<ElectionService>> _loggerMock;
        private readonly ApplicationDbContext _context;
        private readonly IElectionService _electionService;

        public ElectionServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _loggerMock = new Mock<ILogger<ElectionService>>();
            _electionService = new ElectionService(_context, _loggerMock.Object);

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
                    Id = 1,
                    Title = "Test Election 1",
                    Description = "Test Description 1",
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(1),
                    IsActive = true
                },
                new Election
                {
                    Id = 2,
                    Title = "Test Election 2",
                    Description = "Test Description 2",
                    StartDate = DateTime.UtcNow.AddDays(-2),
                    EndDate = DateTime.UtcNow.AddDays(-1),
                    IsActive = false
                }
            };
            _context.Elections.AddRange(elections);

            // Add test positions
            var positions = new List<Position>
            {
                new Position
                {
                    Id = 1,
                    ElectionId = 1,
                    Title = "Test Position 1",
                    DisplayOrder = 1
                },
                new Position
                {
                    Id = 2,
                    ElectionId = 1,
                    Title = "Test Position 2",
                    DisplayOrder = 2
                }
            };
            _context.Positions.AddRange(positions);

            // Add test candidates
            var candidates = new List<Candidate>
            {
                new Candidate
                {
                    Id = 1,
                    PositionId = 1,
                    Name = "Test Candidate 1"
                },
                new Candidate
                {
                    Id = 2,
                    PositionId = 1,
                    Name = "Test Candidate 2"
                },
                new Candidate
                {
                    Id = 3,
                    PositionId = 2,
                    Name = "Test Candidate 3"
                }
            };
            _context.Candidates.AddRange(candidates);

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetElectionByIdAsync_ValidId_ShouldReturnElection()
        {
            // Act
            var result = await _electionService.GetElectionByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
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
        public async Task GetActiveElectionsAsync_ShouldReturnActiveElections()
        {
            // Act
            var result = await _electionService.GetActiveElectionsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Title.Should().Be("Test Election 1");
            result[0].IsActive.Should().BeTrue();
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
            result.Should().BeTrue();
            var savedElection = await _context.Elections
                .FirstOrDefaultAsync(e => e.Title == "New Election");
            savedElection.Should().NotBeNull();
            savedElection.Description.Should().Be("New Description");
        }

        [Fact]
        public async Task UpdateElectionAsync_ValidUpdate_ShouldSucceed()
        {
            // Arrange
            var election = await _context.Elections.FirstAsync(e => e.Id == 1);
            election.Title = "Updated Election";
            election.Description = "Updated Description";

            // Act
            var result = await _electionService.UpdateElectionAsync(election);

            // Assert
            result.Should().BeTrue();
            var updatedElection = await _context.Elections
                .FirstOrDefaultAsync(e => e.Id == 1);
            updatedElection.Should().NotBeNull();
            updatedElection.Title.Should().Be("Updated Election");
            updatedElection.Description.Should().Be("Updated Description");
        }

        [Fact]
        public async Task UpdateElectionAsync_InvalidElection_ShouldFail()
        {
            // Arrange
            var election = new Election
            {
                Id = 999,
                Title = "Invalid Election"
            };

            // Act
            var result = await _electionService.UpdateElectionAsync(election);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteElectionAsync_ValidElection_ShouldSucceed()
        {
            // Act
            var result = await _electionService.DeleteElectionAsync(1);

            // Assert
            result.Should().BeTrue();
            var deletedElection = await _context.Elections
                .FirstOrDefaultAsync(e => e.Id == 1);
            deletedElection.Should().BeNull();
        }

        [Fact]
        public async Task DeleteElectionAsync_InvalidElection_ShouldFail()
        {
            // Act
            var result = await _electionService.DeleteElectionAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetElectionPositionsAsync_ShouldReturnPositions()
        {
            // Act
            var result = await _electionService.GetElectionPositionsAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(p => p.Title == "Test Position 1");
            result.Should().Contain(p => p.Title == "Test Position 2");
        }

        [Fact]
        public async Task GetElectionCandidatesAsync_ShouldReturnCandidates()
        {
            // Act
            var result = await _electionService.GetElectionCandidatesAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().Contain(c => c.Name == "Test Candidate 1");
            result.Should().Contain(c => c.Name == "Test Candidate 2");
            result.Should().Contain(c => c.Name == "Test Candidate 3");
        }

        [Fact]
        public async Task ValidateElectionDatesAsync_ValidDates_ShouldReturnTrue()
        {
            // Arrange
            var election = new Election
            {
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2)
            };

            // Act
            var result = await _electionService.ValidateElectionDatesAsync(election);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateElectionDatesAsync_InvalidDates_ShouldReturnFalse()
        {
            // Arrange
            var election = new Election
            {
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var result = await _electionService.ValidateElectionDatesAsync(election);

            // Assert
            result.Should().BeFalse();
        }
    }
} 