using Microsoft.EntityFrameworkCore;
using VotingSystem_Claude.Data;
using VotingSystem_Claude.Models;
using VotingSystem_Claude.Services.Interfaces;

namespace VotingSystem_Claude.Services
{
    public class ElectionService : IElectionService
    {
        private readonly ApplicationDbContext _context;

        public ElectionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Election>> GetAllElectionsAsync()
        {
            return await _context.Elections
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();
        }

        public async Task<Election> GetElectionByIdAsync(int id)
        {
            return await _context.Elections
                .Include(e => e.Positions)
                .ThenInclude(p => p.Candidates)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Election> GetActiveElectionAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Elections
                .Include(e => e.Positions.OrderBy(p => p.DisplayOrder))
                .ThenInclude(p => p.Candidates)
                .FirstOrDefaultAsync(e => e.IsActive && e.StartDate <= now && e.EndDate >= now);
        }

        public async Task<Election> CreateElectionAsync(Election election)
        {
            _context.Elections.Add(election);
            await _context.SaveChangesAsync();
            return election;
        }

        public async Task<bool> UpdateElectionAsync(Election election)
        {
            _context.Entry(election).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ElectionExists(election.Id))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<bool> DeleteElectionAsync(int id)
        {
            var election = await _context.Elections.FindAsync(id);
            if (election == null)
            {
                return false;
            }

            _context.Elections.Remove(election);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateElectionAsync(int id)
        {
            // First deactivate all elections
            var activeElections = await _context.Elections.Where(e => e.IsActive).ToListAsync();
            foreach (var activeElection in activeElections)
            {
                activeElection.IsActive = false;
            }

            // Then activate the requested one
            var election = await _context.Elections.FindAsync(id);
            if (election == null)
            {
                return false;
            }

            election.IsActive = true;
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> ElectionExists(int id)
        {
            return await _context.Elections.AnyAsync(e => e.Id == id);
        }
    }
}
