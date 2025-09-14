using VotingSystem_Claude.Models;

namespace VotingSystem_Claude.Services.Interfaces
{
    public interface IAdminService
    {
        Task<Admin?> AuthenticateAsync(string username, string password, string ipAddress);
        Task<Admin?> GetAdminByIdAsync(int id);
        Task<Admin?> GetAdminByUsernameAsync(string username);
        Task<bool> CreateAdminAsync(Admin admin, string password);
        Task<bool> UpdateAdminAsync(Admin admin);
        Task<bool> ChangePasswordAsync(int adminId, string currentPassword, string newPassword);
        Task<bool> IsLockedOutAsync(string username);
        Task ResetFailedAttemptsAsync(string username);
        Task<bool> ResetPasswordAsync(string username, string newPassword);
    }
}