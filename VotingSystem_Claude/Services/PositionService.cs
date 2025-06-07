using Microsoft.EntityFrameworkCore;
using VotingSystem_Claude.Data;
using VotingSystem_Claude.Models;
using VotingSystem_Claude.Services.Interfaces;

namespace VotingSystem_Claude.Services
{
    public class PositionService : IPositionService
    {
        private readonly ApplicationDbContext _context;

        public PositionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Position>> GetPositionsByElectionIdAsync(int electionId)
        {
            return await _context.Positions
                .Where(p => p.ElectionId == electionId)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();
        }

        public async Task<Position> GetPositionByIdAsync(int id)
        {
            return await _context.Positions
                .Include(p => p.Candidates)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Position> CreatePositionAsync(Position position)
        {
            _context.Positions.Add(position);
            await _context.SaveChangesAsync();
            return position;
        }

        public async Task<bool> UpdatePositionAsync(Position position)
        {
            _context.Entry(position).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PositionExists(position.Id))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<bool> DeletePositionAsync(int id)
        {
            var position = await _context.Positions.FindAsync(id);
            if (position == null)
            {
                return false;
            }

            _context.Positions.Remove(position);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePositionOrderAsync(List<Position> positions)
        {
            foreach (var position in positions)
            {
                var existingPosition = await _context.Positions.FindAsync(position.Id);
                if (existingPosition != null)
                {
                    existingPosition.DisplayOrder = position.DisplayOrder;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> PositionExists(int id)
        {
            return await _context.Positions.AnyAsync(p => p.Id == id);
        }

        public async Task<List<Position>> GetAllPositionsAsync()
        {
            return await _context.Positions
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();
        }
    }
}
