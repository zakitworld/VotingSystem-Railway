using Microsoft.EntityFrameworkCore;
using Moq;
using VotingSystem_Claude.Data;
using VotingSystem_Claude.Services;
using VotingSystem_Claude.Services.Interfaces;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace VotingSystem_Claude.Tests.Services
{
    public class StudentServiceTests
    {
        private readonly Mock<ILogger<StudentService>> _loggerMock;
        private readonly ApplicationDbContext _context;
        private readonly IStudentService _studentService;

        public StudentServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _loggerMock = new Mock<ILogger<StudentService>>();
            _studentService = new StudentService(_context, _loggerMock.Object);

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
                    Id = 1,
                    StudentId = "2024001",
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    Department = "Computer Science",
                    YearLevel = 1
                },
                new Student
                {
                    Id = 2,
                    StudentId = "2024002",
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@example.com",
                    Department = "Engineering",
                    YearLevel = 2
                }
            };
            _context.Students.AddRange(students);

            // Add test voters
            var voters = new List<Voter>
            {
                new Voter { Id = 1, StudentId = 1 },
                new Voter { Id = 2, StudentId = 2 }
            };
            _context.Voters.AddRange(voters);

            _context.SaveChanges();
        }

        [Fact]
        public async Task GetStudentByStudentIdAsync_ValidId_ShouldReturnStudent()
        {
            // Act
            var result = await _studentService.GetStudentByStudentIdAsync("2024001");

            // Assert
            result.Should().NotBeNull();
            result.StudentId.Should().Be("2024001");
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
        }

        [Fact]
        public async Task GetStudentByStudentIdAsync_InvalidId_ShouldReturnNull()
        {
            // Act
            var result = await _studentService.GetStudentByStudentIdAsync("999999");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetStudentByEmailAsync_ValidEmail_ShouldReturnStudent()
        {
            // Act
            var result = await _studentService.GetStudentByEmailAsync("john.doe@example.com");

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be("john.doe@example.com");
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
        }

        [Fact]
        public async Task GetStudentByEmailAsync_InvalidEmail_ShouldReturnNull()
        {
            // Act
            var result = await _studentService.GetStudentByEmailAsync("invalid@example.com");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateStudentAsync_ValidStudent_ShouldSucceed()
        {
            // Arrange
            var student = new Student
            {
                StudentId = "2024003",
                FirstName = "Alice",
                LastName = "Johnson",
                Email = "alice.johnson@example.com",
                Department = "Mathematics",
                YearLevel = 1
            };

            // Act
            var result = await _studentService.CreateStudentAsync(student);

            // Assert
            result.Should().BeTrue();
            var savedStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == "2024003");
            savedStudent.Should().NotBeNull();
            savedStudent.FirstName.Should().Be("Alice");
        }

        [Fact]
        public async Task CreateStudentAsync_DuplicateStudentId_ShouldFail()
        {
            // Arrange
            var student = new Student
            {
                StudentId = "2024001",
                FirstName = "Bob",
                LastName = "Wilson",
                Email = "bob.wilson@example.com",
                Department = "Physics",
                YearLevel = 1
            };

            // Act
            var result = await _studentService.CreateStudentAsync(student);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateStudentAsync_ValidUpdate_ShouldSucceed()
        {
            // Arrange
            var student = await _context.Students.FirstAsync(s => s.StudentId == "2024001");
            student.FirstName = "Johnny";
            student.LastName = "Doe Jr.";

            // Act
            var result = await _studentService.UpdateStudentAsync(student);

            // Assert
            result.Should().BeTrue();
            var updatedStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == "2024001");
            updatedStudent.Should().NotBeNull();
            updatedStudent.FirstName.Should().Be("Johnny");
            updatedStudent.LastName.Should().Be("Doe Jr.");
        }

        [Fact]
        public async Task UpdateStudentAsync_InvalidStudent_ShouldFail()
        {
            // Arrange
            var student = new Student
            {
                Id = 999,
                StudentId = "999999",
                FirstName = "Invalid",
                LastName = "Student"
            };

            // Act
            var result = await _studentService.UpdateStudentAsync(student);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteStudentAsync_ValidStudent_ShouldSucceed()
        {
            // Arrange
            var studentId = "2024001";

            // Act
            var result = await _studentService.DeleteStudentAsync(studentId);

            // Assert
            result.Should().BeTrue();
            var deletedStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
            deletedStudent.Should().BeNull();
        }

        [Fact]
        public async Task DeleteStudentAsync_InvalidStudent_ShouldFail()
        {
            // Act
            var result = await _studentService.DeleteStudentAsync("999999");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllStudentsAsync_ShouldReturnAllStudents()
        {
            // Act
            var result = await _studentService.GetAllStudentsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(s => s.StudentId == "2024001");
            result.Should().Contain(s => s.StudentId == "2024002");
        }

        [Fact]
        public async Task GetStudentsByDepartmentAsync_ShouldReturnFilteredStudents()
        {
            // Act
            var result = await _studentService.GetStudentsByDepartmentAsync("Computer Science");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Department.Should().Be("Computer Science");
        }

        [Fact]
        public async Task GetStudentsByYearLevelAsync_ShouldReturnFilteredStudents()
        {
            // Act
            var result = await _studentService.GetStudentsByYearLevelAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].YearLevel.Should().Be(1);
        }
    }
} 