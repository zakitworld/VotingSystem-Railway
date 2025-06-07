using VotingSystem_Claude.Models;

namespace VotingSystem_Claude.Services.Interfaces
{
    public interface IVoterService
    {
        Task<List<Voter>> GetAllVotersAsync();
        Task<Voter> GetVoterByIdAsync(int id);
        Task<Voter> GetVoterByCodeAsync(string voterCode);
        Task<Voter> CreateVoterAsync(Voter voter);
        Task<bool> UpdateVoterAsync(Voter voter);
        Task<bool> DeleteVoterAsync(int id);
        Task<bool> ResetVoterStatusAsync(int id);
        Task<bool> VotersExistWithStudentIdsAsync(List<string> studentIds);
        Task<bool> RegenerateVoterCodeAsync(int id);
    }
} 