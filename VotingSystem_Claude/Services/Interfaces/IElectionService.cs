using VotingSystem_Claude.Models;

namespace VotingSystem_Claude.Services.Interfaces
{
    public interface IElectionService
    {
        Task<List<Election>> GetAllElectionsAsync();
        Task<Election> GetElectionByIdAsync(int id);
        Task<Election> GetActiveElectionAsync();
        Task<Election> CreateElectionAsync(Election election);
        Task<bool> UpdateElectionAsync(Election election);
        Task<bool> DeleteElectionAsync(int id);
        Task<bool> ActivateElectionAsync(int id);
    }
} 