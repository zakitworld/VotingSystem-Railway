using Microsoft.EntityFrameworkCore;
using VotingSystem_Claude.Data;
using VotingSystem_Claude.Models;
using VotingSystem_Claude.Services.Interfaces;

namespace VotingSystem_Claude.Services
{
    public class VoterService : IVoterService
    {
        private readonly ApplicationDbContext _context;
        private readonly IVoterCodeService _voterCodeService;

        public VoterService(ApplicationDbContext context, IVoterCodeService voterCodeService)
        {
            _context = context;
            _voterCodeService = voterCodeService;
        }

        public async Task<List<Voter>> GetAllVotersAsync()
        {
            return await _context.Voters
                .Include(v => v.Student)
                .Include(v => v.VoterCode)
                .ToListAsync();
        }

        public async Task<Voter?> GetVoterByIdAsync(int id)
        {
            return await _context.Voters
                .Include(v => v.Student)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<Voter?> GetVoterByCodeAsync(string voterCode)
        {
            return await _context.Voters
                .Include(v => v.Student)
                .FirstOrDefaultAsync(v => v.VoterCode != null && v.VoterCode.Code == voterCode);
        }

        public async Task<Voter> CreateVoterAsync(Voter voter)
        {
            _context.Voters.Add(voter);
            await _context.SaveChangesAsync();
            return voter;
        }

        public async Task<bool> UpdateVoterAsync(Voter voter)
        {
            try
            {
                // Check if the entity is already being tracked
                var existingEntity = _context.ChangeTracker.Entries<Voter>()
                    .FirstOrDefault(e => e.Entity.Id == voter.Id);

                if (existingEntity != null)
                {
                    // If already tracked, update the existing entity's properties
                    existingEntity.CurrentValues.SetValues(voter);
                }
                else
                {
                    // If not tracked, attach and set as modified
                    _context.Entry(voter).State = EntityState.Modified;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await VoterExists(voter.Id))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<bool> DeleteVoterAsync(int id)
        {
            var voter = await _context.Voters.FindAsync(id);
            if (voter == null)
            {
                return false;
            }

            _context.Voters.Remove(voter);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetVoterStatusAsync(int id)
        {
            var voter = await _context.Voters.FindAsync(id);
            if (voter == null)
            {
                return false;
            }

            voter.HasVoted = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VotersExistWithStudentIdsAsync(List<string> studentIds)
        {
            return await _context.Students
                .AnyAsync(s => studentIds.Contains(s.StudentId) && s.Voter != null);
        }

        public async Task<bool> RegenerateVoterCodeAsync(int id)
        {
            var voter = await _context.Voters.FindAsync(id);
            if (voter == null)
            {
                return false;
            }

            var newCode = await _voterCodeService.GenerateVoterCodeAsync();
            voter.VoterCode = new VoterCode 
            { 
                Code = newCode,
                GeneratedAt = DateTime.UtcNow,
                IsUsed = false
            };
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> VoterExists(int id)
        {
            return await _context.Voters.AnyAsync(v => v.Id == id);
        }
    }
}
