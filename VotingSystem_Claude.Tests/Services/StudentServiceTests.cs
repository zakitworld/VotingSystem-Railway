using Microsoft.EntityFrameworkCore;
using Moq;
using VotingSystem_Claude.Data;
using VotingSystem_Claude.Services;
using VotingSystem_Claude.Services.Interfaces;
using Xunit;
using FluentAssertions;
using VotingSystem_Claude.Models;
using Microsoft.Extensions.Logging;

namespace VotingSystem_Claude.Tests.Services
{
    public class StudentServiceTests
    {
        private readonly Mock<IVoterService> _voterServiceMock;
        private readonly Mock<IVoterCodeService> _voterCodeServiceMock;
        private readonly ApplicationDbContext _context;
        private readonly IStudentService _studentService;

        public StudentServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _voterServiceMock = new Mock<IVoterService>();
            _voterCodeServiceMock = new Mock<IVoterCodeService>();
            
            _studentService = new StudentService(_context, _voterServiceMock.Object, _voterCodeServiceMock.Object);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Add test students
            var students = new List<Student>
            {
                new Student
                {
                    StudentId = "2024001",
                    FullName = "John Doe",
                    Class = "Computer Science"
                },
                new Student
                {
                    StudentId = "2024002",
                    FullName = "Jane Smith",
                    Class = "Engineering"
                }
            };
            _context.Students.AddRange(students);
            _context.SaveChanges();

            // Add test voters
            foreach (var student in _context.Students)
            {
                _context.Voters.Add(new Voter { StudentId = student.Id });
            }
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetStudentByStudentIdAsync_ValidId_ShouldReturnStudent()
        {
            // Act
            var result = await _studentService.GetStudentByStudentIdAsync("2024001");

            // Assert
            result.Should().NotBeNull();
            result!.StudentId.Should().Be("2024001");
            result.FullName.Should().Be("John Doe");
        }

        [Fact]
        public async Task CreateStudentAsync_ValidStudent_ShouldSucceed()
        {
            // Arrange
            var student = new Student
            {
                StudentId = "2024003",
                FullName = "Alice Johnson",
                Class = "Mathematics"
            };
            _voterCodeServiceMock.Setup(s => s.GenerateVoterCodeAsync()).ReturnsAsync("CODE123");

            // Act
            var result = await _studentService.CreateStudentAsync(student);

            // Assert
            result.Should().NotBeNull();
            var savedStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == "2024003");
            savedStudent.Should().NotBeNull();
            _voterServiceMock.Verify(s => s.CreateVoterAsync(It.IsAny<Voter>()), Times.Once);
        }

        [Fact]
        public async Task UpdateStudentAsync_ValidUpdate_ShouldSucceed()
        {
            // Arrange
            var student = await _context.Students.FirstAsync(s => s.StudentId == "2024001");
            student.FullName = "Johnny Doe";

            // Act
            var result = await _studentService.UpdateStudentAsync(student);

            // Assert
            result.Should().BeTrue();
            var updatedStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == "2024001");
            updatedStudent.Should().NotBeNull();
            updatedStudent!.FullName.Should().Be("Johnny Doe");
        }

        [Fact]
        public async Task DeleteStudentAsync_ValidStudent_ShouldSucceed()
        {
            // Arrange
            var student = await _context.Students.FirstAsync(s => s.StudentId == "2024001");

            // Act
            var result = await _studentService.DeleteStudentAsync(student.Id);

            // Assert
            result.Should().BeTrue();
            var deletedStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == student.Id);
            deletedStudent.Should().BeNull();
        }

        [Fact]
        public async Task GetAllStudentsAsync_ShouldReturnAllStudents()
        {
            // Act
            var result = await _studentService.GetAllStudentsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }
    }
}