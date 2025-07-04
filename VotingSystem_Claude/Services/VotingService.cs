using Microsoft.EntityFrameworkCore;
using VotingSystem_Claude.Data;
using VotingSystem_Claude.Models;
using VotingSystem_Claude.Services.Interfaces;

namespace VotingSystem_Claude.Services
{
    public class VotingService : IVotingService
    {
        private readonly ApplicationDbContext _context;

        public VotingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SubmitVotesAsync(int voterId, Dictionary<int, int> positionCandidateVotes, int electionId)
        {
            // Validate voter
            var voter = await _context.Voters
                .Include(v => v.Votes)
                .FirstOrDefaultAsync(v => v.Id == voterId);

            if (voter == null)
            {
                return false;
            }

            // Check if voter has already voted in this election
            if (voter.Votes != null && voter.Votes.Any(v => v.ElectionId == electionId))
            {
                return false;
            }

            // Get the election to ensure it's active
            var election = await _context.Elections
                .Include(e => e.Positions)
                .ThenInclude(p => p.Candidates) // Include candidates for validation
                .FirstOrDefaultAsync(e => e.Id == electionId);

            if (election == null || !election.IsActive ||
                election.StartDate > DateTime.UtcNow ||
                election.EndDate < DateTime.UtcNow)
            {
                return false;
            }

            // Validate positions and candidates
            foreach (var kvp in positionCandidateVotes)
            {
                var positionId = kvp.Key;
                var candidateId = kvp.Value;

                var position = election.Positions.FirstOrDefault(p => p.Id == positionId);
                if (position == null)
                {
                    return false;
                }

                var candidate = position.Candidates.FirstOrDefault(c => c.Id == candidateId);
                if (candidate == null)
                {
                    return false;
                }
            }

            // Create votes
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                // Mark voter as having voted (if this property exists in your Voter model)
                voter.HasVoted = true;
                voter.LastLoginTime = DateTime.UtcNow;

                foreach (var kvp in positionCandidateVotes)
                {
                    var positionId = kvp.Key;
                    var candidateId = kvp.Value;

                    var vote = new Vote
                    {
                        VoterId = voterId,
                        CandidateId = candidateId,
                        PositionId = positionId,
                        ElectionId = electionId,
                        Timestamp = DateTime.UtcNow
                    };

                    _context.Votes.Add(vote);
                }

                await _context.SaveChangesAsync();
                return true;
            });
        }

        public async Task<bool> HasVotedInElectionAsync(int voterId, int electionId)
        {
            return await _context.Votes
                .AnyAsync(v => v.VoterId == voterId && v.ElectionId == electionId);
        }

        public async Task<List<Vote>> GetVoterVotesAsync(int voterId, int electionId)
        {
            return await _context.Votes
                .Include(v => v.Candidate)
                .Include(v => v.Position)
                .Where(v => v.VoterId == voterId && v.ElectionId == electionId)
                .ToListAsync();
        }

        public async Task<Dictionary<int, List<Vote>>> GetElectionResultsAsync(int electionId)
        {
            var votes = await _context.Votes
                .Include(v => v.Candidate)
                .Include(v => v.Position)
                .Where(v => v.ElectionId == electionId)
                .ToListAsync();

            return votes.GroupBy(v => v.PositionId)
                       .ToDictionary(g => g.Key, g => g.ToList());
        }

        public async Task<int> GetTotalVotesForCandidateAsync(int candidateId, int electionId)
        {
            return await _context.Votes
                .CountAsync(v => v.CandidateId == candidateId && v.ElectionId == electionId);
        }

        public async Task<bool> CanVoterVoteAsync(int voterId, int electionId)
        {
            // Check if voter exists
            var voter = await _context.Voters.FindAsync(voterId);
            if (voter == null)
            {
                return false;
            }

            // Check if election is active and within voting period
            var election = await _context.Elections.FindAsync(electionId);
            if (election == null || !election.IsActive ||
                election.StartDate > DateTime.UtcNow ||
                election.EndDate < DateTime.UtcNow)
            {
                return false;
            }

            // Check if voter has already voted
            var hasVoted = await HasVotedInElectionAsync(voterId, electionId);
            return !hasVoted;
        }
    }
}