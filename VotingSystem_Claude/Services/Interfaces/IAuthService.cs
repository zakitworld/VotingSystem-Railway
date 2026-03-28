using VotingSystem_Claude.Models;

namespace VotingSystem_Claude.Services.Interfaces
{
    public interface IAuthService
    {
        Task<Voter?> AuthenticateVoterAsync(string voterCode);
        Task<Admin?> AuthenticateAdminAsync(string username, string password, string ipAddress);
        Task<Voter?> GetAuthenticatedVoterAsync();
        Task<Admin?> GetAuthenticatedAdminAsync();
        Task LogoutAsync();
        Task<bool> IsAdminAsync();
        Task<bool> IsSessionExpiredAsync();
        Task RefreshSessionAsync();
    }
}
