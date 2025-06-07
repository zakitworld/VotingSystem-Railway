using VotingSystem_Claude.Models;

namespace VotingSystem_Claude.Services.Interfaces
{
    public interface IAuthService
    {
        Task<Voter> AuthenticateVoterAsync(string voterCode);
        Task<bool> AuthenticateAdminAsync(string username, string password);
        Task<Voter> GetAuthenticatedVoterAsync();
        Task LogoutAsync();
        Task<bool> IsAdminAsync();
    }
} 