using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VotingSystem_Claude.Data;
using VotingSystem_Claude.Middleware;
using VotingSystem_Claude.Services.Interfaces;

namespace VotingSystem_Claude.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditService> _logger;

        public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogLoginAttemptAsync(string username, bool successful, string ipAddress, string userAgent)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = username,
                    Action = successful ? "LOGIN_SUCCESS" : "LOGIN_FAILED",
                    EntityType = "Authentication",
                    EntityId = username,
                    Details = JsonSerializer.Serialize(new
                    {
                        Username = username,
                        Successful = successful,
                        AttemptTime = DateTime.UtcNow
                    }),
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Timestamp = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Login attempt logged for user {Username}: {Result}", 
                    username, successful ? "SUCCESS" : "FAILED");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log login attempt for user {Username}", username);
            }
        }

        public async Task LogVoteSubmissionAsync(int voterId, int electionId, string ipAddress, bool successful)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = voterId.ToString(),
                    Action = successful ? "VOTE_CAST" : "VOTE_FAILED",
                    EntityType = "Vote",
                    EntityId = $"{electionId}",
                    Details = JsonSerializer.Serialize(new
                    {
                        VoterId = voterId,
                        ElectionId = electionId,
                        Successful = successful,
                        VoteTime = DateTime.UtcNow
                    }),
                    IpAddress = ipAddress,
                    Timestamp = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Vote submission logged for voter {VoterId} in election {ElectionId}: {Result}", 
                    voterId, electionId, successful ? "SUCCESS" : "FAILED");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log vote submission for voter {VoterId}", voterId);
            }
        }

        public async Task LogAdminActionAsync(string adminUsername, string action, string details, string ipAddress)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = adminUsername,
                    Action = $"ADMIN_{action.ToUpper()}",
                    EntityType = "Admin",
                    EntityId = adminUsername,
                    Details = JsonSerializer.Serialize(new
                    {
                        AdminUsername = adminUsername,
                        Action = action,
                        Details = details,
                        ActionTime = DateTime.UtcNow
                    }),
                    IpAddress = ipAddress,
                    Timestamp = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Admin action logged: {AdminUsername} performed {Action}", 
                    adminUsername, action);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log admin action for {AdminUsername}: {Action}", 
                    adminUsername, action);
            }
        }

        public async Task LogSecurityEventAsync(string eventType, string description, string ipAddress, string? userId = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = userId ?? "SYSTEM",
                    Action = $"SECURITY_{eventType.ToUpper()}",
                    EntityType = "Security",
                    EntityId = eventType,
                    Details = JsonSerializer.Serialize(new
                    {
                        EventType = eventType,
                        Description = description,
                        UserId = userId,
                        EventTime = DateTime.UtcNow
                    }),
                    IpAddress = ipAddress,
                    Timestamp = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogWarning("Security event logged: {EventType} - {Description} (User: {UserId}, IP: {IpAddress})", 
                    eventType, description, userId ?? "Unknown", ipAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log security event {EventType}", eventType);
            }
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, string? userId = null)
        {
            try
            {
                var query = _context.AuditLogs.AsQueryable();

                if (fromDate.HasValue)
                    query = query.Where(log => log.Timestamp >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(log => log.Timestamp <= toDate.Value);

                if (!string.IsNullOrEmpty(userId))
                    query = query.Where(log => log.UserId == userId);

                return await query
                    .OrderByDescending(log => log.Timestamp)
                    .Take(1000) // Limit to last 1000 entries
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve audit logs");
                return new List<AuditLog>();
            }
        }

        public async Task<List<AuditLog>> GetFailedLoginAttemptsAsync(DateTime? fromDate = null)
        {
            try
            {
                var query = _context.AuditLogs
                    .Where(log => log.Action == "LOGIN_FAILED");

                if (fromDate.HasValue)
                    query = query.Where(log => log.Timestamp >= fromDate.Value);
                else
                    query = query.Where(log => log.Timestamp >= DateTime.UtcNow.AddDays(-7)); // Last 7 days by default

                return await query
                    .OrderByDescending(log => log.Timestamp)
                    .Take(100)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve failed login attempts");
                return new List<AuditLog>();
            }
        }
    }
}