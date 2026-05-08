using Microsoft.EntityFrameworkCore;
using VotingSystem_Claude.Data;
using VotingSystem_Claude.Models;
using VotingSystem_Claude.Services.Interfaces;
using VotingSystem_Claude.Services;
using OfficeOpenXml;
using CsvHelper;
using System.Globalization;
using System.IO;
using CsvHelper.Configuration;

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
                VoterCode = new VoterCode 
                { 
                    Code = await _voterCodeService.GenerateVoterCodeAsync(),
                    GeneratedAt = DateTime.UtcNow,
                    IsUsed = false
                }
            };
            await _voterService.CreateVoterAsync(voter);

            return student;
        }

        public async Task<bool> UpdateStudentAsync(Student student)
        {
            var existingStudent = await _context.Students.FindAsync(student.Id);
            if (existingStudent == null)
            {
                return false; // Student not found
            }

            // Update properties of the existing (tracked) entity
            _context.Entry(existingStudent).CurrentValues.SetValues(student);

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

        public async Task<(int Imported, int Skipped, string Message)> ImportStudentsFromExcelAsync(Stream fileStream)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension.Rows;
            
            int imported = 0;
            int skipped = 0;

            for (int row = 2; row <= rowCount; row++)
            {
                var studentId = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                var fullName = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                var className = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                var house = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                var gender = worksheet.Cells[row, 5].Value?.ToString()?.Trim();

                if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(fullName))
                {
                    skipped++;
                    continue;
                }

                if (await _context.Students.AnyAsync(s => s.StudentId == studentId))
                {
                    skipped++;
                    continue;
                }

                var student = new Student
                {
                    StudentId = studentId,
                    FullName = fullName,
                    Class = className,
                    House = house,
                    Gender = gender,
                    CreatedAt = DateTime.UtcNow
                };

                await CreateStudentAsync(student);
                imported++;
            }

            return (imported, skipped, $"Import completed: {imported} imported, {skipped} skipped.");
        }

        public async Task<(int Imported, int Skipped, string Message)> ImportStudentsFromCsvAsync(Stream fileStream)
        {
            using var reader = new StreamReader(fileStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            
            int imported = 0;
            int skipped = 0;

            var records = csv.GetRecords<dynamic>();
            foreach (var record in records)
            {
                var dict = record as IDictionary<string, object>;
                if (dict == null) continue;

                // Try to find values by common header names
                string studentId = GetValue(dict, "StudentId", "ID", "Student ID");
                string fullName = GetValue(dict, "FullName", "Name", "Full Name");
                string className = GetValue(dict, "Class", "Grade");
                string house = GetValue(dict, "House");
                string gender = GetValue(dict, "Gender", "Sex");

                if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(fullName))
                {
                    skipped++;
                    continue;
                }

                if (await _context.Students.AnyAsync(s => s.StudentId == studentId))
                {
                    skipped++;
                    continue;
                }

                var student = new Student
                {
                    StudentId = studentId,
                    FullName = fullName,
                    Class = className,
                    House = house,
                    Gender = gender,
                    CreatedAt = DateTime.UtcNow
                };

                await CreateStudentAsync(student);
                imported++;
            }

            return (imported, skipped, $"Import completed: {imported} imported, {skipped} skipped.");
        }

        private string GetValue(IDictionary<string, object> dict, params string[] keys)
        {
            foreach (var key in keys)
            {
                var matchedKey = dict.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.OrdinalIgnoreCase));
                if (matchedKey != null) return dict[matchedKey]?.ToString() ?? "";
            }
            return "";
        }

        public async Task<byte[]> ExportStudentsToExcelAsync()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Students");
            
            var students = await GetAllStudentsAsync();
            
            // Headers
            worksheet.Cells[1, 1].Value = "Student ID";
            worksheet.Cells[1, 2].Value = "Full Name";
            worksheet.Cells[1, 3].Value = "Class";
            worksheet.Cells[1, 4].Value = "House";
            worksheet.Cells[1, 5].Value = "Gender";
            worksheet.Cells[1, 6].Value = "Voter Code";

            for (int i = 0; i < students.Count; i++)
            {
                var student = students[i];
                worksheet.Cells[i + 2, 1].Value = student.StudentId;
                worksheet.Cells[i + 2, 2].Value = student.FullName;
                worksheet.Cells[i + 2, 3].Value = student.Class;
                worksheet.Cells[i + 2, 4].Value = student.House;
                worksheet.Cells[i + 2, 5].Value = student.Gender;
                worksheet.Cells[i + 2, 6].Value = student.Voter?.VoterCode?.Code ?? "N/A";
            }

            worksheet.Cells.AutoFitColumns();
            return await package.GetAsByteArrayAsync();
        }

        public async Task<string> ExportStudentsToCsvAsync()
        {
            var students = await GetAllStudentsAsync();
            using var writer = new StringWriter();
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            
            csv.WriteHeader<StudentExportModel>();
            await csv.NextRecordAsync();
            
            foreach (var student in students)
            {
                var model = new StudentExportModel
                {
                    StudentId = student.StudentId,
                    FullName = student.FullName,
                    Class = student.Class,
                    House = student.House,
                    Gender = student.Gender,
                    VoterCode = student.Voter?.VoterCode?.Code ?? "N/A"
                };
                csv.WriteRecord(model);
                await csv.NextRecordAsync();
            }

            return writer.ToString();
        }

        private class StudentExportModel
        {
            public string StudentId { get; set; } = "";
            public string FullName { get; set; } = "";
            public string? Class { get; set; }
            public string? House { get; set; }
            public string? Gender { get; set; }
            public string VoterCode { get; set; } = "";
        }
    }
}