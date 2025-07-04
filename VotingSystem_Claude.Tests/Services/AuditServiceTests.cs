using Microsoft.EntityFrameworkCore;
using Moq;
using VotingSystem_Claude.Data;
using VotingSystem_Claude.Services;
using VotingSystem_Claude.Services.Interfaces;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace VotingSystem_Claude.Tests.Services
{
    public class AuditServiceTests
    {
        private readonly Mock<ILogger<AuditService>> _loggerMock;
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public AuditServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _loggerMock = new Mock<ILogger<AuditService>>();
            _auditService = new AuditService(_context, _loggerMock.Object);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Add test audit logs
            var auditLogs = new List<AuditLog>
            {
                new AuditLog
                {
                    Id = 1,
                    UserId = "user1",
                    Action = "Login",
                    Timestamp = DateTime.UtcNow.AddHours(-2),
                    Details = "User logged in successfully"
                },
                new AuditLog
                {
                    Id = 2,
                    UserId = "user1",
                    Action = "Vote",
                    Timestamp = DateTime.UtcNow.AddHours(-1),
                    Details = "User cast vote for election 1"
                },
                new AuditLog
                {
                    Id = 3,
                    UserId = "user2",
                    Action = "Login",
                    Timestamp = DateTime.UtcNow.AddMinutes(-30),
                    Details = "User logged in successfully"
                }
            };
            _context.AuditLogs.AddRange(auditLogs);

            _context.SaveChanges();
        }

        [Fact]
        public async Task LogActionAsync_ValidLog_ShouldSucceed()
        {
            // Arrange
            var auditLog = new AuditLog
            {
                UserId = "user3",
                Action = "Logout",
                Timestamp = DateTime.UtcNow,
                Details = "User logged out"
            };

            // Act
            var result = await _auditService.LogActionAsync(auditLog);

            // Assert
            result.Should().BeTrue();
            var savedLog = await _context.AuditLogs
                .FirstOrDefaultAsync(l => l.UserId == "user3" && l.Action == "Logout");
            savedLog.Should().NotBeNull();
            savedLog.Details.Should().Be("User logged out");
        }

        [Fact]
        public async Task GetAuditLogsByUserIdAsync_ShouldReturnUserLogs()
        {
            // Act
            var result = await _auditService.GetAuditLogsByUserIdAsync("user1");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(l => l.Action == "Login");
            result.Should().Contain(l => l.Action == "Vote");
        }

        [Fact]
        public async Task GetAuditLogsByActionAsync_ShouldReturnActionLogs()
        {
            // Act
            var result = await _auditService.GetAuditLogsByActionAsync("Login");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(l => l.UserId == "user1");
            result.Should().Contain(l => l.UserId == "user2");
        }

        [Fact]
        public async Task GetAuditLogsByDateRangeAsync_ShouldReturnFilteredLogs()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddHours(-3);
            var endDate = DateTime.UtcNow.AddHours(-1);

            // Act
            var result = await _auditService.GetAuditLogsByDateRangeAsync(startDate, endDate);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(l => l.UserId == "user1" && l.Action == "Login");
            result.Should().Contain(l => l.UserId == "user1" && l.Action == "Vote");
        }

        [Fact]
        public async Task GetAuditLogsByDateRangeAsync_NoLogsInRange_ShouldReturnEmpty()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-1);
            var endDate = DateTime.UtcNow.AddDays(-1);

            // Act
            var result = await _auditService.GetAuditLogsByDateRangeAsync(startDate, endDate);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAuditLogsAsync_ShouldReturnAllLogs()
        {
            // Act
            var result = await _auditService.GetAllAuditLogsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().Contain(l => l.UserId == "user1" && l.Action == "Login");
            result.Should().Contain(l => l.UserId == "user1" && l.Action == "Vote");
            result.Should().Contain(l => l.UserId == "user2" && l.Action == "Login");
        }

        [Fact]
        public async Task GetAuditLogsByDetailsAsync_ShouldReturnFilteredLogs()
        {
            // Act
            var result = await _auditService.GetAuditLogsByDetailsAsync("successfully");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(l => l.UserId == "user1" && l.Action == "Login");
            result.Should().Contain(l => l.UserId == "user2" && l.Action == "Login");
        }

        [Fact]
        public async Task GetAuditLogsByDetailsAsync_NoMatchingLogs_ShouldReturnEmpty()
        {
            // Act
            var result = await _auditService.GetAuditLogsByDetailsAsync("nonexistent");

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAuditLogsByUserAndActionAsync_ShouldReturnFilteredLogs()
        {
            // Act
            var result = await _auditService.GetAuditLogsByUserAndActionAsync("user1", "Login");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].UserId.Should().Be("user1");
            result[0].Action.Should().Be("Login");
        }

        [Fact]
        public async Task GetAuditLogsByUserAndActionAsync_NoMatchingLogs_ShouldReturnEmpty()
        {
            // Act
            var result = await _auditService.GetAuditLogsByUserAndActionAsync("user1", "Logout");

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
} 