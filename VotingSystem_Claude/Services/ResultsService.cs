using Microsoft.EntityFrameworkCore;
using VotingSystem_Claude.Data;
using VotingSystem_Claude.Models;
using VotingSystem_Claude.Services.Interfaces;

namespace VotingSystem_Claude.Services
{
    public class ResultsService : IResultsService
    {
        private readonly ApplicationDbContext _context;

        public ResultsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<int, List<CandidateResult>>> GetElectionResultsAsync(int electionId)
        {
            var results = new Dictionary<int, List<CandidateResult>>();

            var positions = await _context.Positions
                .Where(p => p.ElectionId == electionId)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();

            foreach (var position in positions)
            {
                var candidates = await _context.Candidates
                    .Where(c => c.PositionId == position.Id)
                    .ToListAsync();

                var candidateResults = new List<CandidateResult>();

                foreach (var candidate in candidates)
                {
                    var voteCount = await _context.Votes
                        .CountAsync(v => v.CandidateId == candidate.Id);

                    candidateResults.Add(new CandidateResult
                    {
                        CandidateId = candidate.Id,
                        FullName = candidate.FullName,
                        Class = candidate.Class,
                        ImagePath = candidate.ImagePath,
                        VoteCount = voteCount
                    });
                }

                // Sort by vote count in descending order
                candidateResults = candidateResults
                    .OrderByDescending(c => c.VoteCount)
                    .ToList();

                results.Add(position.Id, candidateResults);
            }

            return results;
        }

        public async Task<int> GetTotalVotersAsync(int electionId)
        {
            // Get the count of unique voters who voted in this election
            return await _context.Votes
                .Where(v => v.ElectionId == electionId)
                .Select(v => v.VoterId)
                .Distinct()
                .CountAsync();
        }

        public async Task<int> GetTotalEligibleVotersAsync()
        {
            return await _context.Voters.CountAsync();
        }

        public async Task<Dictionary<string, int>> GetVoterClassBreakdownAsync(int electionId)
        {
            var breakdown = new Dictionary<string, int>();

            var classData = await _context.Votes
                .Where(v => v.ElectionId == electionId)
                .Select(v => v.Voter.Student.Class)
                .Distinct()
                .ToListAsync();

            foreach (var className in classData)
            {
                var count = await _context.Votes
                    .Where(v => v.ElectionId == electionId && v.Voter.Student.Class == className)
                    .Select(v => v.VoterId)
                    .Distinct()
                    .CountAsync();

                breakdown.Add(className, count);
            }

            return breakdown;
        }
    }

    public class CandidateResult
    {
        public int CandidateId { get; set; }
        public string FullName { get; set; }
        public string Class { get; set; }
        public string ImagePath { get; set; }
        public int VoteCount { get; set; }
    }
}
