using Microsoft.EntityFrameworkCore;
using VotingSystem_Claude.Data;
using VotingSystem_Claude.Models;
using VotingSystem_Claude.Services.Interfaces;

namespace VotingSystem_Claude.Services
{
    public class VoterCodeService : IVoterCodeService
    {
        private readonly ApplicationDbContext _context;

        public VoterCodeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateVoterCodeAsync()
        {
            string code;
            do
            {
                code = GenerateCode();
            } while (await _context.Voters.AnyAsync(v => v.VoterCode != null && v.VoterCode.Code == code));

            return code;
        }

        public async Task<bool> ValidateVoterCodeAsync(string voterCode)
        {
            if (string.IsNullOrWhiteSpace(voterCode))
            {
                return false;
            }

            return await _context.Voters.AnyAsync(v => v.VoterCode != null && v.VoterCode.Code == voterCode);
        }

        public async Task<bool> AssignVoterCodeAsync(int voterId, string voterCode)
        {
            var voter = await _context.Voters.FindAsync(voterId);
            if (voter == null)
            {
                return false;
            }

            // Check if the code is already in use
            if (await _context.Voters.AnyAsync(v => v.VoterCode != null && v.VoterCode.Code == voterCode && v.Id != voterId))
            {
                return false;
            }

            voter.VoterCode = new VoterCode 
            { 
                Code = voterCode,
                GeneratedAt = DateTime.UtcNow,
                IsUsed = false
            };
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RevokeVoterCodeAsync(int voterId)
        {
            var voter = await _context.Voters.FindAsync(voterId);
            if (voter == null)
            {
                return false;
            }

            voter.VoterCode = null;
            await _context.SaveChangesAsync();
            return true;
        }

        private string GenerateCode()
        {
            // Generate a 6-digit alphanumeric code
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
        }

        public async Task<List<Voter>> GetVotersWithoutCodesAsync()
        {
            var studentsWithoutCodes = await _context.Students
                .Where(s => s.Voter == null)
                .ToListAsync();

            var voters = new List<Voter>();
            foreach (var student in studentsWithoutCodes)
            {
                string code;
                do
                {
                    code = GenerateCode();
                } while (await _context.Voters.AnyAsync(v => v.VoterCode != null && v.VoterCode.Code == code));

                var voter = new Voter
                {
                    StudentId = student.Id,
                    VoterCode = new VoterCode 
                    { 
                        Code = code,
                        GeneratedAt = DateTime.UtcNow,
                        IsUsed = false
                    },
                    HasVoted = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Voters.Add(voter);
                voters.Add(voter);
            }

            await _context.SaveChangesAsync();
            return voters;
        }
    }
}
