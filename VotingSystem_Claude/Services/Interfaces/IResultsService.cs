using VotingSystem_Claude.Models;

namespace VotingSystem_Claude.Services.Interfaces
{
    public interface IResultsService
    {
        Task<Dictionary<int, List<CandidateResult>>> GetElectionResultsAsync(int electionId);
        Task<int> GetTotalVotersAsync(int electionId);
        Task<int> GetTotalEligibleVotersAsync();
        Task<Dictionary<string, int>> GetVoterClassBreakdownAsync(int electionId);
    }
} 