using Microsoft.EntityFrameworkCore;
using VotingSystem_Claude.Data;
using VotingSystem_Claude.Models;
using VotingSystem_Claude.Services.Interfaces;
using VotingSystem_Claude.Services;

namespace VotingSystem_Claude.Services
{
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IVoterService _voterService;
        private readonly IVoterCodeService _voterCodeService;

        public StudentService(ApplicationDbContext context, IVoterService voterService, IVoterCodeService voterCodeService)
        {
            _context = context;
            _voterService = voterService;
            _voterCodeService = voterCodeService;
        }

        public async Task<List<Student>> GetAllStudentsAsync()
        {
            return await _context.Students
                .Include(s => s.Voter)
                .ToListAsync();
        }

        public async Task<Student?> GetStudentByIdAsync(int id)
        {
            return await _context.Students
                .Include(s => s.Voter)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Student?> GetStudentByStudentIdAsync(string studentId)
        {
            return await _context.Students
                .Include(s => s.Voter)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
        }

        public async Task<Student> CreateStudentAsync(Student student)
        {
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            // Automatically create a Voter record for the new student
            var voter = new Voter
            {
                StudentId = student.Id,
                HasVoted = false,
                CreatedAt = DateTime.UtcNow,
                VoterCode = await _voterCodeService.GenerateVoterCodeAsync() // Generate a code
            };
            await _voterService.CreateVoterAsync(voter);

            return student;
        }

        public async Task<bool> UpdateStudentAsync(Student student)
        {
            _context.Entry(student).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(student.Id))
                {
                    return false;
                }
                throw;
            }
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return false;
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Student>> GetAvailableStudentsAsync()
        {
            return await _context.Students
                .Include(s => s.Voter)
                .Where(s => s.Voter == null)
                .ToListAsync();
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}