using Human_Resource_Management_System.Models;

namespace Human_Resource_Management_System.Services
{
    public interface IUserService
    {
        Task<User?> AuthenticateAsync(string email, string password);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> CreateUserAsync(User user, string password);
        Task<List<User>> GetAllUsersAsync();
        
        // Enhanced User Registration Methods
        Task<(bool Success, string Message, string? UserId)> CreateCompleteUserAsync(UserRegistrationViewModel model);
        Task<UserRegistrationConfirmationViewModel?> GetRegistrationConfirmationAsync(string userId);
        Task<bool> SendRegistrationConfirmationEmailAsync(string email, UserRegistrationConfirmationViewModel confirmationData);
    }
}