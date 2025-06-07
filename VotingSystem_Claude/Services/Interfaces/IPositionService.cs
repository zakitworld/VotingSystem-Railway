using VotingSystem_Claude.Models;

namespace VotingSystem_Claude.Services.Interfaces
{
    public interface IPositionService
    {
        Task<List<Position>> GetPositionsByElectionIdAsync(int electionId);
        Task<Position> GetPositionByIdAsync(int id);
        Task<Position> CreatePositionAsync(Position position);
        Task<bool> UpdatePositionAsync(Position position);
        Task<bool> DeletePositionAsync(int id);
        Task<bool> UpdatePositionOrderAsync(List<Position> positions);
        Task<List<Position>> GetAllPositionsAsync();
    }
} 