using Human_Resource_Management_System.Data;
using Human_Resource_Management_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Human_Resource_Management_System.Services
{
    public class UserService : IUserService
    {
        private readonly HrmsDbContext _context;

        public UserService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);
            
            if (user == null || !user.IsActive)
                return null;

            // Verify password using BCrypt
            if (BCrypt.Net.BCrypt.Verify(password, user.Password))
                return user;

            return null;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> CreateUserAsync(User user, string password)
        {
            try
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(password);
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.FirstName)
                .ToListAsync();
        }
    }
}