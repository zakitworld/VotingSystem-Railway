using VotingSystem_Claude.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace VotingSystem_Claude.Services.Interfaces
{
    public interface ICandidateService
    {
        Task<List<Candidate>> GetCandidatesByPositionIdAsync(int positionId);
        Task<Candidate?> GetCandidateByIdAsync(int id);
        Task<Candidate> CreateCandidateAsync(Candidate candidate);
        Task<bool> UpdateCandidateAsync(Candidate candidate);
        Task<bool> DeleteCandidateAsync(int id);
        Task<int> GetCandidateVoteCountAsync(int candidateId);
        Task<bool> ValidateUniqueNamePerPositionAsync(string fullName, int positionId, int? excludeCandidateId = null);
        Task<List<Candidate>> SearchCandidatesAsync(string searchTerm, int? electionId = null);
        Task<string?> UploadCandidateImageAsync(IBrowserFile imageFile);
        Task<List<Candidate>> GetAllCandidatesAsync();
    }
}
