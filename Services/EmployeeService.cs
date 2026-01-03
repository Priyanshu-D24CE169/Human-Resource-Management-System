using hrms.Data;
using hrms.Models;
using Microsoft.EntityFrameworkCore;

namespace hrms.Services
{
    public interface IEmployeeService
    {
        Task<string> GenerateLoginIdAsync(string firstName, string lastName, DateTime joiningDate);
        string GenerateRandomPassword();
        Task<int> GetNextSerialNumberAsync(int year);
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateLoginIdAsync(string firstName, string lastName, DateTime joiningDate)
        {
            var companySettings = await _context.CompanySettings.FirstOrDefaultAsync();
            var companyCode = companySettings?.CompanyCode ?? "OI";
            
            var year = joiningDate.Year;
            var firstLetter = firstName.Substring(0, 1).ToUpper();
            var lastLetter = lastName.Substring(0, 1).ToUpper();
            
            var serialNumber = await GetNextSerialNumberAsync(year);
            
            var loginId = $"{companyCode}{firstLetter}{lastLetter}{year}{serialNumber:D4}";
            
            return loginId;
        }

        public async Task<int> GetNextSerialNumberAsync(int year)
        {
            var lastEmployee = await _context.Employees
                .Where(e => e.YearOfJoining == year)
                .OrderByDescending(e => e.SerialNumber)
                .FirstOrDefaultAsync();

            return lastEmployee != null ? lastEmployee.SerialNumber + 1 : 1;
        }

        public string GenerateRandomPassword()
        {
            // Define character sets to ensure all requirements are met
            const string uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lowercase = "abcdefghijkmnpqrstuvwxyz";
            const string digits = "23456789";
            const string special = "@#$!%^&*";
            
            var random = new Random();
            var password = new List<char>();

            // Ensure at least one character from each required set
            password.Add(uppercase[random.Next(uppercase.Length)]);
            password.Add(lowercase[random.Next(lowercase.Length)]);
            password.Add(digits[random.Next(digits.Length)]);
            password.Add(special[random.Next(special.Length)]);

            // Fill the rest with random characters from all sets
            const string allChars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789@#$!%^&*";
            for (int i = password.Count; i < 12; i++)
            {
                password.Add(allChars[random.Next(allChars.Length)]);
            }

            // Shuffle the password to avoid predictable patterns
            return new string(password.OrderBy(x => random.Next()).ToArray());
        }
    }
}
