using Microsoft.EntityFrameworkCore;
using VotingSystem_Claude.Data;
using VotingSystem_Claude.Models;
using VotingSystem_Claude.Services.Interfaces;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;

namespace VotingSystem_Claude.Services
{
    public class CandidateService : ICandidateService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CandidateService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<List<Candidate>> GetCandidatesByPositionIdAsync(int positionId)
        {
            return await _context.Candidates
                .Where(c => c.PositionId == positionId)
                .ToListAsync();
        }

        public async Task<Candidate> GetCandidateByIdAsync(int id)
        {
            return await _context.Candidates
                .Include(c => c.Student)
                .Include(c => c.Position)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Candidate> CreateCandidateAsync(Candidate candidate)
        {
            _context.Candidates.Add(candidate);
            await _context.SaveChangesAsync();
            return candidate;
        }

        public async Task<bool> UpdateCandidateAsync(Candidate candidate)
        {
            _context.Entry(candidate).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CandidateExists(candidate.Id))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<bool> DeleteCandidateAsync(int id)
        {
            var candidate = await _context.Candidates.FindAsync(id);
            if (candidate == null)
            {
                return false;
            }

            _context.Candidates.Remove(candidate);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCandidateVoteCountAsync(int candidateId)
        {
            return await _context.Votes
                .CountAsync(v => v.CandidateId == candidateId);
        }

        private async Task<bool> CandidateExists(int id)
        {
            return await _context.Candidates.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> ValidateUniqueNamePerPositionAsync(string fullName, int positionId, int? excludeCandidateId = null)
        {
            return !await _context.Candidates
                .AnyAsync(c => c.FullName == fullName &&
                              c.PositionId == positionId &&
                              (excludeCandidateId == null || c.Id != excludeCandidateId));
        }

        public async Task<List<Candidate>> SearchCandidatesAsync(string searchTerm, int? electionId = null)
        {
            var query = _context.Candidates.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => c.FullName.Contains(searchTerm) ||
                                        c.Class.Contains(searchTerm));
            }

            if (electionId.HasValue)
            {
                query = query.Include(c => c.Position)
                             .Where(c => c.Position != null && c.Position.ElectionId == electionId);
            }

            return await query.Include(c => c.Position).ToListAsync();
        }

        public async Task<string> UploadCandidateImageAsync(IBrowserFile imageFile)
        {
            if (imageFile == null)
            {
                return null;
            }

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "candidates");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.Name;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.OpenReadStream().CopyToAsync(fileStream);
            }

            return Path.Combine("images", "candidates", uniqueFileName).Replace("\\", "/");
        }
    }
}
