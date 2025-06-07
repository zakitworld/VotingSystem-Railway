using VotingSystem_Claude.Models;

namespace VotingSystem_Claude.Services.Interfaces
{
    public interface IVoterCodeService
    {
        Task<string> GenerateVoterCodeAsync();
        Task<bool> ValidateVoterCodeAsync(string voterCode);
        Task<bool> AssignVoterCodeAsync(int voterId, string voterCode);
        Task<bool> RevokeVoterCodeAsync(int voterId);
    }
} 