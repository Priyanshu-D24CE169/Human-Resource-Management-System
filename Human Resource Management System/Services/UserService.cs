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
    }
}