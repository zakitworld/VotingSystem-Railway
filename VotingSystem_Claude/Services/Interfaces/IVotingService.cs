using VotingSystem_Claude.Models;

namespace VotingSystem_Claude.Services.Interfaces
{
    public interface IVotingService
    {
        Task<bool> SubmitVotesAsync(int voterId, Dictionary<int, int> positionCandidateVotes, int electionId);
        Task<bool> HasVotedInElectionAsync(int voterId, int electionId);
        Task<List<Vote>> GetVoterVotesAsync(int voterId, int electionId);
        Task<Dictionary<int, List<Vote>>> GetElectionResultsAsync(int electionId);
        Task<int> GetTotalVotesForCandidateAsync(int candidateId, int electionId);
        Task<bool> CanVoterVoteAsync(int voterId, int electionId);
    }
} 