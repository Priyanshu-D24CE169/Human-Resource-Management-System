using Human_Resource_Management_System.Data;
using Human_Resource_Management_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Human_Resource_Management_System.Services
{
    public class UserService : IUserService
    {
        private readonly HrmsDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(HrmsDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation($"?? Attempting authentication for email: {email}");
                
                var user = await GetUserByEmailAsync(email);
                
                if (user == null)
                {
                    _logger.LogWarning($"? User not found for email: {email}");
                    return null;
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning($"? User account is inactive for email: {email}");
                    return null;
                }

                _logger.LogInformation($"?? Password verification for {email}:");
                _logger.LogInformation($"   Provided password: {password}");
                _logger.LogInformation($"   Stored hash: {user.Password.Substring(0, Math.Min(30, user.Password.Length))}...");
                
                // Test BCrypt verification
                bool isPasswordValid = false;
                try
                {
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
                    _logger.LogInformation($"   BCrypt verification result: {isPasswordValid}");
                }
                catch (Exception bcryptEx)
                {
                    _logger.LogError(bcryptEx, $"? BCrypt verification error for {email}");
                    return null;
                }
                
                if (isPasswordValid)
                {
                    _logger.LogInformation($"? Authentication successful for email: {email}, Role: {user.Role}");
                    return user;
                }
                else
                {
                    _logger.LogWarning($"? Invalid password for email: {email}");
                    
                    // Generate a test hash for comparison
                    var testHash = BCrypt.Net.BCrypt.HashPassword(password);
                    _logger.LogInformation($"   Test hash would be: {testHash.Substring(0, Math.Min(30, testHash.Length))}...");
                    
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"?? Authentication error for email: {email}");
                return null;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                _logger.LogDebug($"?? Looking up user by email: {email}");
                
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                
                if (user != null)
                {
                    _logger.LogDebug($"?? Found user: {user.FirstName} {user.LastName}, Role: {user.Role}, Active: {user.IsActive}");
                }
                else
                {
                    _logger.LogDebug($"?? No user found for email: {email}");
                }
                
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"?? Error retrieving user by email: {email}");
                return null;
            }
        }

        public async Task<bool> CreateUserAsync(User user, string password)
        {
            try
            {
                _logger.LogInformation($"?? Creating new user: {user.Email}");
                
                user.Password = BCrypt.Net.BCrypt.HashPassword(password);
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"? User created successfully: {user.Email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"?? Error creating user: {user.Email}");
                return false;
            }
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                _logger.LogDebug("?? Retrieving all active users");
                
                var users = await _context.Users
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.FirstName)
                    .ToListAsync();
                
                _logger.LogDebug($"?? Found {users.Count} active users");
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "?? Error retrieving all users");
                return new List<User>();
            }
        }

        // Enhanced User Registration Methods
        public async Task<(bool Success, string Message, string? UserId)> CreateCompleteUserAsync(UserRegistrationViewModel model)
        {
            try
            {
                _logger.LogInformation($"?? Creating complete user profile for: {model.FirstName} {model.LastName}");

                // Validate age (18-65)
                if (model.Age < 18)
                {
                    return (false, "User must be at least 18 years old", null);
                }
                if (model.Age > 65)
                {
                    return (false, "User must be under 65 years old", null);
                }

                // Check if email already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());
                if (existingUser != null)
                {
                    return (false, "A user with this email already exists", null);
                }

                // Generate employee code
                var employeeCode = await GenerateEmployeeCodeAsync(model.FirstName, model.LastName, model.HireDate);
                
                // Create user with complete profile
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = model.Role,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                // Create corresponding employee record
                var employee = new Employee
                {
                    EmployeeCode = employeeCode,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Department = model.Department,
                    Position = model.Position,
                    HireDate = model.HireDate,
                    Salary = model.Salary,
                    Status = model.Status,
                    IsRegistrationComplete = true,
                    Address = model.FullAddress,
                    EmergencyContactName = model.EmergencyContactName,
                    EmergencyContactPhone = model.EmergencyContactPhone,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    Nationality = model.Nationality,
                    ProfileImagePath = model.ProfilePicturePath,
                    CreatedDate = DateTime.Now
                };

                // Add to database
                _context.Users.Add(user);
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"? Complete user profile created: {employeeCode} - {user.FirstName} {user.LastName}");
                
                return (true, "User registration completed successfully!", user.UserId.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"?? Error creating complete user profile for: {model.FirstName} {model.LastName}");
                return (false, $"Registration failed: {ex.Message}", null);
            }
        }

        public async Task<UserRegistrationConfirmationViewModel?> GetRegistrationConfirmationAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString() == userId);
                if (user == null) return null;

                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
                if (employee == null) return null;

                return new UserRegistrationConfirmationViewModel
                {
                    UserId = user.UserId.ToString(),
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = employee.Phone,
                    FullAddress = employee.Address ?? "",
                    Department = employee.Department,
                    Position = employee.Position,
                    EmployeeCode = employee.EmployeeCode,
                    HireDate = employee.HireDate,
                    Salary = employee.Salary,
                    Status = employee.Status,
                    RegistrationDate = user.CreatedDate,
                    EmailConfirmed = true, // We can add email confirmation later
                    ProfilePicturePath = employee.ProfileImagePath
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving registration confirmation for user ID: {userId}");
                return null;
            }
        }

        public async Task<bool> SendRegistrationConfirmationEmailAsync(string email, UserRegistrationConfirmationViewModel confirmationData)
        {
            try
            {
                _logger.LogInformation($"?? Sending registration confirmation email to: {email}");

                var subject = "Registration Successful - Welcome to Odoo India HRMS";
                var body = GenerateRegistrationConfirmationEmail(confirmationData);

                // Here you would integrate with your email service
                // For now, just log the email content
                _logger.LogInformation($"?? Registration confirmation email prepared for: {confirmationData.FirstName} {confirmationData.LastName}");
                _logger.LogInformation($"?? Employee Code: {confirmationData.EmployeeCode}");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"?? Error sending registration confirmation email to: {email}");
                return false;
            }
        }

        private async Task<string> GenerateEmployeeCodeAsync(string firstName, string lastName, DateTime hireDate)
        {
            try
            {
                // Generate Odoo India format: OI + FirstTwoLetters + LastTwoLetters + Year + SerialNumber
                var prefix = "OI";
                var firstInitials = firstName.Length >= 2 ? firstName.Substring(0, 2).ToUpper() : firstName.ToUpper().PadRight(2, 'X');
                var lastInitials = lastName.Length >= 2 ? lastName.Substring(0, 2).ToUpper() : lastName.ToUpper().PadRight(2, 'X');
                var year = hireDate.Year;
                
                // Get the next serial number for this year
                var yearPrefix = $"{prefix}{firstInitials}{lastInitials}{year}";
                var existingCodes = await _context.Employees
                    .Where(e => e.EmployeeCode.StartsWith(yearPrefix))
                    .CountAsync();
                
                var serialNumber = (existingCodes + 1).ToString("D4");
                
                return $"{yearPrefix}{serialNumber}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating employee code");
                return $"OI{firstName.Substring(0, Math.Min(2, firstName.Length)).ToUpper()}{lastName.Substring(0, Math.Min(2, lastName.Length)).ToUpper()}{DateTime.Now.Year}{new Random().Next(1000, 9999)}";
            }
        }

        private string GenerateRegistrationConfirmationEmail(UserRegistrationConfirmationViewModel data)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>Registration Successful - Odoo India HRMS</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; box-shadow: 0 4px 20px rgba(0,0,0,0.1); overflow: hidden; }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 40px 30px; text-align: center; }}
                        .header h1 {{ margin: 0; font-size: 2rem; }}
                        .content {{ padding: 30px; }}
                        .welcome {{ font-size: 1.1rem; color: #333; margin-bottom: 20px; }}
                        .details-card {{ background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                        .detail-row {{ display: flex; justify-content: space-between; margin: 10px 0; }}
                        .label {{ font-weight: bold; color: #555; }}
                        .value {{ color: #333; }}
                        .employee-code {{ font-size: 1.2rem; font-weight: bold; color: #667eea; text-align: center; padding: 15px; background: #e8f0fe; border-radius: 8px; margin: 20px 0; }}
                        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 0.9rem; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>?? Welcome to Odoo India HRMS!</h1>
                            <p>Registration Successfully Completed</p>
                        </div>
                        
                        <div class='content'>
                            <div class='welcome'>
                                Dear <strong>{data.FirstName} {data.LastName}</strong>,
                                <br><br>
                                Congratulations! Your registration has been completed successfully. Welcome to the Odoo India team!
                            </div>
                            
                            <div class='employee-code'>
                                Your Employee Code: <strong>{data.EmployeeCode}</strong>
                            </div>
                            
                            <div class='details-card'>
                                <h3>?? Personal Information</h3>
                                <div class='detail-row'>
                                    <span class='label'>Full Name:</span>
                                    <span class='value'>{data.FirstName} {data.LastName}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Email:</span>
                                    <span class='value'>{data.Email}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Phone:</span>
                                    <span class='value'>{data.Phone}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Address:</span>
                                    <span class='value'>{data.FullAddress}</span>
                                </div>
                            </div>
                            
                            <div class='details-card'>
                                <h3>?? Employment Details</h3>
                                <div class='detail-row'>
                                    <span class='label'>Department:</span>
                                    <span class='value'>{data.Department}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Position:</span>
                                    <span class='value'>{data.Position}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Hire Date:</span>
                                    <span class='value'>{data.HireDate:MMMM dd, yyyy}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Status:</span>
                                    <span class='value'>{data.Status}</span>
                                </div>
                            </div>
                            
                            <div class='details-card'>
                                <h3>?? Next Steps</h3>
                                <ul>
                                    <li>You can now login to the HRMS system using your email and password</li>
                                    <li>Complete your profile if any additional information is needed</li>
                                    <li>Familiarize yourself with the system features</li>
                                    <li>Contact HR if you have any questions</li>
                                </ul>
                            </div>
                        </div>
                        
                        <div class='footer'>
                            <p>Registration completed on: <strong>{data.RegistrationDate:MMMM dd, yyyy}</strong></p>
                            <p>Welcome to the Odoo India family! ??</p>
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
}