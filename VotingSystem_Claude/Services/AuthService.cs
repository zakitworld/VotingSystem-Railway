using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using VotingSystem_Claude.Models;
using VotingSystem_Claude.Services.Interfaces;

namespace VotingSystem_Claude.Services
{
    public class AuthService : IAuthService
    {
        private readonly IVoterService _voterService;
        private readonly IAdminService _adminService;
        private readonly IAuditService _auditService;
        private readonly ProtectedSessionStorage _sessionStorage;
        private const string SessionKey = "VoterId";
        private const string AdminSessionKey = "AdminId";
        private const string SessionTimeKey = "SessionTime";
        private const int SessionTimeoutMinutes = 30;

        public AuthService(IVoterService voterService, IAdminService adminService, IAuditService auditService, ProtectedSessionStorage sessionStorage)
        {
            _voterService = voterService;
            _adminService = adminService;
            _auditService = auditService;
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

        public async Task<Admin?> AuthenticateAdminAsync(string username, string password, string ipAddress)
        {
            var admin = await _adminService.AuthenticateAsync(username, password, ipAddress);
            if (admin != null)
            {
                await _sessionStorage.SetAsync(AdminSessionKey, admin.Id);
                await _sessionStorage.SetAsync(SessionTimeKey, DateTime.UtcNow);
            }
            return admin;
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

        public async Task<Admin?> GetAuthenticatedAdminAsync()
        {
            try
            {
                if (await IsSessionExpiredAsync())
                    return null;

                var result = await _sessionStorage.GetAsync<int>(AdminSessionKey);
                if (result.Success)
                {
                    return await _adminService.GetAdminByIdAsync(result.Value);
                }
            }
            catch
            {
                // Session may be invalid or expired
            }

            return null;
        }

        public async Task<bool> IsAdminAsync()
        {
            var admin = await GetAuthenticatedAdminAsync();
            return admin != null;
        }

        public async Task<bool> IsSessionExpiredAsync()
        {
            try
            {
                var sessionTimeResult = await _sessionStorage.GetAsync<DateTime>(SessionTimeKey);
                if (sessionTimeResult.Success)
                {
                    var sessionTime = sessionTimeResult.Value;
                    return DateTime.UtcNow.Subtract(sessionTime).TotalMinutes > SessionTimeoutMinutes;
                }
            }
            catch { }

            return true; // Assume expired if unable to check
        }

        public async Task RefreshSessionAsync()
        {
            await _sessionStorage.SetAsync(SessionTimeKey, DateTime.UtcNow);
        }
    }
}
