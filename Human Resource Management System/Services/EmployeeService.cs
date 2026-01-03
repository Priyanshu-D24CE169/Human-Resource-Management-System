using Human_Resource_Management_System.Data;
using Human_Resource_Management_System.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Human_Resource_Management_System.Services
{
    public interface IEmployeeService
    {
        Task<Employee?> GetEmployeeByIdAsync(int id);
        Task<Employee?> GetEmployeeByEmailAsync(string email);
        Task<Employee?> GetEmployeeByCodeAsync(string employeeCode);
        Task<Employee?> GetEmployeeByTokenAsync(string token);
        Task<List<Employee>> GetAllEmployeesAsync();
        Task<Employee> CreateEmployeeAsync(EmployeeViewModel model);
        Task<bool> UpdateEmployeeAsync(int id, EmployeeViewModel model);
        Task<bool> DeleteEmployeeAsync(int id);
        Task<bool> CompleteEmployeeRegistrationAsync(EmployeeRegistrationViewModel model);
        Task<string> GenerateRegistrationTokenAsync(int employeeId);
        Task<bool> IsRegistrationTokenValidAsync(string token);
        Task<bool> UpdateEmployeeProfileAsync(int employeeId, EmployeeProfileEditViewModel model);
        string GenerateNextEmployeeCode(string firstName, string lastName, DateTime hireDate);
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly HrmsDbContext _context;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(HrmsDbContext context, ILogger<EmployeeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            try
            {
                return await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving employee by ID: {id}");
                return null;
            }
        }

        public async Task<Employee?> GetEmployeeByEmailAsync(string email)
        {
            try
            {
                return await _context.Employees.FirstOrDefaultAsync(e => e.Email.ToLower() == email.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving employee by email: {email}");
                return null;
            }
        }

        public async Task<Employee?> GetEmployeeByCodeAsync(string employeeCode)
        {
            try
            {
                return await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving employee by code: {employeeCode}");
                return null;
            }
        }

        public async Task<Employee?> GetEmployeeByTokenAsync(string token)
        {
            try
            {
                return await _context.Employees
                    .FirstOrDefaultAsync(e => e.RegistrationToken == token && 
                                            e.RegistrationTokenExpiry > DateTime.Now &&
                                            !e.IsRegistrationComplete);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving employee by token: {token}");
                return null;
            }
        }

        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            try
            {
                return await _context.Employees
                    .OrderBy(e => e.FirstName)
                    .ThenBy(e => e.LastName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all employees");
                return new List<Employee>();
            }
        }

        public async Task<Employee> CreateEmployeeAsync(EmployeeViewModel model)
        {
            try
            {
                _logger.LogInformation($"?? Creating employee: {model.FirstName} {model.LastName}");
                _logger.LogInformation($"   Email: {model.Email}");
                _logger.LogInformation($"   Department: {model.Department}");
                _logger.LogInformation($"   Position: {model.Position}");
                _logger.LogInformation($"   Salary: {model.Salary}");

                _logger.LogInformation("?? Generating employee code...");
                var employeeCode = GenerateNextEmployeeCode(model.FirstName, model.LastName, model.HireDate);
                _logger.LogInformation($"? Employee code generated: {employeeCode}");

                _logger.LogInformation("?? Generating registration token...");
                var registrationToken = GenerateRegistrationToken();
                _logger.LogInformation($"? Registration token generated: {registrationToken.Substring(0, 8)}...");

                var employee = new Employee
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone ?? "",
                    Department = model.Department,
                    Position = model.Position,
                    HireDate = model.HireDate,
                    Salary = model.Salary,
                    Status = model.Status,
                    EmployeeCode = employeeCode,
                    IsRegistrationComplete = false,
                    RegistrationToken = registrationToken,
                    RegistrationTokenExpiry = DateTime.Now.AddDays(7),
                    CreatedDate = DateTime.Now
                };

                _logger.LogInformation("?? Saving employee to database...");
                _context.Employees.Add(employee);
                var saveResult = await _context.SaveChangesAsync();
                _logger.LogInformation($"? Database save result: {saveResult} rows affected");

                _logger.LogInformation($"?? Employee created successfully with ID: {employee.EmployeeId}");
                return employee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"?? Error creating employee: {model.FirstName} {model.LastName}");
                _logger.LogError($"   Exception Type: {ex.GetType().Name}");
                _logger.LogError($"   Exception Message: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"   Inner Exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task<bool> UpdateEmployeeAsync(int id, EmployeeViewModel model)
        {
            try
            {
                var employee = await GetEmployeeByIdAsync(id);
                if (employee == null)
                {
                    _logger.LogWarning($"Employee not found for update: {id}");
                    return false;
                }

                employee.FirstName = model.FirstName;
                employee.LastName = model.LastName;
                employee.Email = model.Email;
                employee.Phone = model.Phone;
                employee.Department = model.Department;
                employee.Position = model.Position;
                employee.HireDate = model.HireDate;
                employee.Salary = model.Salary;
                employee.Status = model.Status;
                employee.ModifiedDate = DateTime.Now;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Employee updated successfully: {employee.EmployeeCode} - {employee.FullName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating employee ID: {id}");
                return false;
            }
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            try
            {
                var employee = await GetEmployeeByIdAsync(id);
                if (employee == null)
                {
                    _logger.LogWarning($"Employee not found for deletion: {id}");
                    return false;
                }

                // Soft delete by setting status to Inactive
                employee.Status = "Inactive";
                employee.ModifiedDate = DateTime.Now;
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Employee deactivated successfully: {employee.EmployeeCode} - {employee.FullName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deactivating employee ID: {id}");
                return false;
            }
        }

        public async Task<bool> CompleteEmployeeRegistrationAsync(EmployeeRegistrationViewModel model)
        {
            try
            {
                var employee = await GetEmployeeByTokenAsync(model.Token);
                if (employee == null)
                {
                    _logger.LogWarning($"Invalid or expired registration token: {model.Token}");
                    return false;
                }

                // Update employee profile
                employee.Phone = model.Phone;
                employee.Address = model.Address;
                employee.EmergencyContactName = model.EmergencyContactName;
                employee.EmergencyContactPhone = model.EmergencyContactPhone;
                employee.DateOfBirth = model.DateOfBirth;
                employee.Gender = model.Gender;
                employee.Nationality = model.Nationality;
                employee.IsRegistrationComplete = true;
                employee.RegistrationToken = null;
                employee.RegistrationTokenExpiry = null;
                employee.ModifiedDate = DateTime.Now;

                // Handle profile image if uploaded
                if (model.ProfileImage != null && model.ProfileImage.Length > 0)
                {
                    var imagePath = await SaveProfileImageAsync(employee.EmployeeCode, model.ProfileImage);
                    employee.ProfileImagePath = imagePath;
                }

                // Create user account for login
                var user = new User
                {
                    Email = employee.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Role = "Employee",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Employee registration completed successfully: {employee.EmployeeCode} - {employee.FullName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing employee registration for token: {model.Token}");
                return false;
            }
        }

        public async Task<string> GenerateRegistrationTokenAsync(int employeeId)
        {
            try
            {
                var token = Guid.NewGuid().ToString("N") + DateTime.Now.Ticks.ToString("x");
                
                var employee = await GetEmployeeByIdAsync(employeeId);
                if (employee != null)
                {
                    employee.RegistrationToken = token;
                    employee.RegistrationTokenExpiry = DateTime.Now.AddDays(7);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation($"Registration token generated for employee: {employee.EmployeeCode}");
                }
                
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating registration token for employee: {employeeId}");
                return string.Empty;
            }
        }
        
        private string GenerateRegistrationToken()
        {
            return Guid.NewGuid().ToString("N") + DateTime.Now.Ticks.ToString("x");
        }

        public async Task<bool> IsRegistrationTokenValidAsync(string token)
        {
            try
            {
                var employee = await GetEmployeeByTokenAsync(token);
                return employee != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating registration token: {token}");
                return false;
            }
        }

        public string GenerateNextEmployeeCode(string firstName, string lastName, DateTime hireDate)
        {
            try
            {
                // Get the year from hire date
                var year = hireDate.Year;
                
                // Count existing employees for that year to get next serial number
                var existingCount = _context.Employees
                    .Count(e => e.EmployeeCode.Contains(year.ToString()));
                
                var nextSerial = existingCount + 1;
                
                return Employee.GenerateEmployeeCode(firstName, lastName, hireDate, nextSerial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating employee code");
                // Fallback to a simple format
                var year = hireDate.Year;
                var random = new Random();
                var serial = random.Next(1, 9999);
                return Employee.GenerateEmployeeCode(firstName, lastName, hireDate, serial);
            }
        }

        public async Task<bool> UpdateEmployeeProfileAsync(int employeeId, EmployeeProfileEditViewModel model)
        {
            try
            {
                var employee = await GetEmployeeByIdAsync(employeeId);
                if (employee == null)
                {
                    _logger.LogWarning($"Employee not found for profile update: {employeeId}");
                    return false;
                }

                employee.Phone = model.Phone;
                employee.Address = model.Address;
                employee.EmergencyContactName = model.EmergencyContactName;
                employee.EmergencyContactPhone = model.EmergencyContactPhone;
                employee.DateOfBirth = model.DateOfBirth;
                employee.Gender = model.Gender;
                employee.Nationality = model.Nationality;
                employee.ModifiedDate = DateTime.Now;

                // Handle profile image if uploaded
                if (model.ProfileImage != null && model.ProfileImage.Length > 0)
                {
                    var imagePath = await SaveProfileImageAsync(employee.EmployeeCode, model.ProfileImage);
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        employee.ProfileImagePath = imagePath;
                    }
                }

                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Employee profile updated successfully: {employee.EmployeeCode} - {employee.FullName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating employee profile: {employeeId}");
                return false;
            }
        }

        private string GenerateSecureToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var tokenBytes = new byte[32];
            rng.GetBytes(tokenBytes);
            return Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        private async Task<string> SaveProfileImageAsync(string employeeCode, IFormFile profileImage)
        {
            try
            {
                var uploadsFolder = Path.Combine("wwwroot", "uploads", "profiles");
                Directory.CreateDirectory(uploadsFolder);

                var fileExtension = Path.GetExtension(profileImage.FileName);
                var fileName = $"{employeeCode}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await profileImage.CopyToAsync(stream);

                return $"/uploads/profiles/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving profile image for employee: {employeeCode}");
                return string.Empty;
            }
        }
    }
}