using VotingSystem_Claude.Models;

namespace VotingSystem_Claude.Services.Interfaces
{
    public interface IStudentService
    {
        Task<List<Student>> GetAllStudentsAsync();
        Task<Student?> GetStudentByIdAsync(int id);
        Task<Student?> GetStudentByStudentIdAsync(string studentId);
        Task<Student> CreateStudentAsync(Student student);
        Task<bool> UpdateStudentAsync(Student student);
        Task<bool> DeleteStudentAsync(int id);
        Task<List<Student>> GetAvailableStudentsAsync();
    }
} 