using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using VotingSystem_Claude.Models;
using VotingSystem_Claude.Services.Interfaces;

namespace VotingSystem_Claude.Services
{
    public class AuthService : IAuthService
    {
        private readonly IVoterService _voterService;
        private readonly ProtectedSessionStorage _sessionStorage;
        private const string SessionKey = "VoterId";
        private const string AdminSessionKey = "IsAdmin";

        public AuthService(IVoterService voterService, ProtectedSessionStorage sessionStorage)
        {
            _voterService = voterService;
            _sessionStorage = sessionStorage;
        }

        public async Task<Voter> AuthenticateVoterAsync(string voterCode)
        {
            if (string.IsNullOrWhiteSpace(voterCode))
            {
                return null;
            }

            var voter = await _voterService.GetVoterByCodeAsync(voterCode);
            if (voter != null)
            {
                // Update last login time
                voter.LastLoginTime = DateTime.UtcNow;
                await _voterService.UpdateVoterAsync(voter);

                // Store voter ID in session
                await _sessionStorage.SetAsync(SessionKey, voter.Id);
            }

            return voter;
        }

        public async Task<bool> AuthenticateAdminAsync(string username, string password)
        {
            // For a real application, you would check against a database
            // This is a simplified implementation for demo purposes
            if (username == "admin" && password == "School@2025")
            {
                await _sessionStorage.SetAsync(AdminSessionKey, true);
                return true;
            }
            return false;
        }

        public async Task<Voter> GetAuthenticatedVoterAsync()
        {
            try
            {
                var result = await _sessionStorage.GetAsync<int>(SessionKey);
                if (result.Success)
                {
                    return await _voterService.GetVoterByIdAsync(result.Value);
                }
            }
            catch
            {
                // Session may be invalid or expired
            }

            return null;
        }

        public async Task LogoutAsync()
        {
            await _sessionStorage.DeleteAsync(SessionKey);
            await _sessionStorage.DeleteAsync(AdminSessionKey);
        }

        public async Task<bool> IsAdminAsync()
        {
            try
            {
                var result = await _sessionStorage.GetAsync<bool>(AdminSessionKey);
                return result.Success && result.Value;
            }
            catch
            {
                return false;
            }
        }
    }
}
