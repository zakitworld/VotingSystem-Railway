using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using VotingSystem_Claude.Data;
using VotingSystem_Claude.Models;
using VotingSystem_Claude.Services.Interfaces;

namespace VotingSystem_Claude.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private const int MaxFailedAttempts = 5;
        private const int LockoutMinutes = 30;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Admin?> AuthenticateAsync(string username, string password, string ipAddress)
        {
            var admin = await GetAdminByUsernameAsync(username);
            if (admin == null || !admin.IsActive)
                return null;

            // Check if account is locked out
            if (await IsLockedOutAsync(username))
                return null;

            // Verify password
            if (!VerifyPassword(password, admin.PasswordHash))
            {
                // Increment failed attempts
                admin.FailedLoginAttempts++;
                if (admin.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    admin.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                }
                await _context.SaveChangesAsync();
                return null;
            }

            // Reset failed attempts on successful login
            admin.FailedLoginAttempts = 0;
            admin.LockoutEnd = null;
            admin.LastLoginTime = DateTime.UtcNow;
            admin.LastLoginIp = ipAddress;
            await _context.SaveChangesAsync();

            return admin;
        }

        public async Task<Admin?> GetAdminByIdAsync(int id)
        {
            return await _context.Admins.FindAsync(id);
        }

        public async Task<Admin?> GetAdminByUsernameAsync(string username)
        {
            return await _context.Admins
                .FirstOrDefaultAsync(a => a.Username == username);
        }

        public async Task<bool> CreateAdminAsync(Admin admin, string password)
        {
            admin.PasswordHash = HashPassword(password);
            _context.Admins.Add(admin);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAdminAsync(Admin admin)
        {
            _context.Admins.Update(admin);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ChangePasswordAsync(int adminId, string currentPassword, string newPassword)
        {
            var admin = await GetAdminByIdAsync(adminId);
            if (admin == null || !VerifyPassword(currentPassword, admin.PasswordHash))
                return false;

            admin.PasswordHash = HashPassword(newPassword);
            return await UpdateAdminAsync(admin);
        }

        public async Task<bool> IsLockedOutAsync(string username)
        {
            var admin = await GetAdminByUsernameAsync(username);
            return admin != null && admin.LockoutEnd.HasValue && admin.LockoutEnd > DateTime.UtcNow;
        }

        public async Task ResetFailedAttemptsAsync(string username)
        {
            var admin = await GetAdminByUsernameAsync(username);
            if (admin != null)
            {
                admin.FailedLoginAttempts = 0;
                admin.LockoutEnd = null;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ResetPasswordAsync(string username, string newPassword)
        {
            var admin = await GetAdminByUsernameAsync(username);
            if (admin == null)
                return false;

            admin.PasswordHash = HashPassword(newPassword);
            admin.FailedLoginAttempts = 0;
            admin.LockoutEnd = null;
            admin.IsActive = true;
            
            return await UpdateAdminAsync(admin);
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var salt = Guid.NewGuid().ToString();
            var saltedPassword = password + salt;
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(hashedBytes) + ":" + salt;
        }

        private static bool VerifyPassword(string password, string hashedPassword)
        {
            var parts = hashedPassword.Split(':');
            if (parts.Length != 2) return false;

            var hash = parts[0];
            var salt = parts[1];

            using var sha256 = SHA256.Create();
            var saltedPassword = password + salt;
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            var computedHash = Convert.ToBase64String(hashedBytes);

            return hash == computedHash;
        }
    }
}