using VotingSystem_Claude.Middleware;

namespace VotingSystem_Claude.Services.Interfaces
{
    public interface IAuditService
    {
        Task LogLoginAttemptAsync(string username, bool successful, string ipAddress, string userAgent);
        Task LogVoteSubmissionAsync(int voterId, int electionId, string ipAddress, bool successful);
        Task LogAdminActionAsync(string adminUsername, string action, string details, string ipAddress);
        Task LogSecurityEventAsync(string eventType, string description, string ipAddress, string? userId = null);
        Task<List<AuditLog>> GetAuditLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, string? userId = null);
        Task<List<AuditLog>> GetFailedLoginAttemptsAsync(DateTime? fromDate = null);
    }
}