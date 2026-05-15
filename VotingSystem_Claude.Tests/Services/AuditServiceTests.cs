using Microsoft.EntityFrameworkCore;
using Moq;
using VotingSystem_Claude.Data;
using VotingSystem_Claude.Services;
using VotingSystem_Claude.Services.Interfaces;
using Xunit;
using FluentAssertions;
using VotingSystem_Claude.Models;
using VotingSystem_Claude.Middleware;
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
                    UserId = "user1",
                    Action = "LOGIN_SUCCESS",
                    EntityType = "Authentication",
                    EntityId = "user1",
                    Timestamp = DateTime.UtcNow.AddHours(-2),
                    Details = "User logged in successfully"
                },
                new AuditLog
                {
                    UserId = "user1",
                    Action = "VOTE_CAST",
                    EntityType = "Vote",
                    EntityId = "1",
                    Timestamp = DateTime.UtcNow.AddHours(-1),
                    Details = "User cast vote"
                },
                new AuditLog
                {
                    UserId = "user2",
                    Action = "LOGIN_FAILED",
                    EntityType = "Authentication",
                    EntityId = "user2",
                    Timestamp = DateTime.UtcNow.AddMinutes(-30),
                    Details = "Login failed"
                }
            };
            _context.AuditLogs.AddRange(auditLogs);

            _context.SaveChanges();
        }

        [Fact]
        public async Task LogLoginAttemptAsync_ShouldSucceed()
        {
            // Act
            await _auditService.LogLoginAttemptAsync("testuser", true, "127.0.0.1", "TestAgent");

            // Assert
            var log = await _context.AuditLogs.FirstOrDefaultAsync(l => l.UserId == "testuser" && l.Action == "LOGIN_SUCCESS");
            log.Should().NotBeNull();
            log!.IpAddress.Should().Be("127.0.0.1");
        }

        [Fact]
        public async Task GetAuditLogsAsync_ShouldReturnFilteredLogs()
        {
            // Act
            var result = await _auditService.GetAuditLogsAsync(userId: "user1");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(l => l.Action == "LOGIN_SUCCESS");
            result.Should().Contain(l => l.Action == "VOTE_CAST");
        }

        [Fact]
        public async Task GetFailedLoginAttemptsAsync_ShouldReturnCorrectLogs()
        {
            // Act
            var result = await _auditService.GetFailedLoginAttemptsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Action.Should().Be("LOGIN_FAILED");
        }
    }
}