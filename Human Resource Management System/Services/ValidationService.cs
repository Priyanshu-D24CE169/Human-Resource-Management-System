using Human_Resource_Management_System.Models;
using System.ComponentModel.DataAnnotations;

namespace Human_Resource_Management_System.Services
{
    public interface IValidationService
    {
        ValidationResult ValidateEmployeeRegistration(EmployeeRegistrationViewModel model);
        ValidationResult ValidateEmployeeCreation(EmployeeViewModel model);
        ValidationResult ValidateProfileCompletion(Employee employee);
        List<string> GetPasswordStrengthErrors(string password);
        bool IsValidPhoneNumber(string phoneNumber);
        bool IsValidEmail(string email);
    }

    public class ValidationService : IValidationService
    {
        private readonly ILogger<ValidationService> _logger;

        public ValidationService(ILogger<ValidationService> logger)
        {
            _logger = logger;
        }

        public ValidationResult ValidateEmployeeRegistration(EmployeeRegistrationViewModel model)
        {
            var errors = new List<string>();

            // Age validation
            var age = CalculateAge(model.DateOfBirth);
            if (age < 18)
            {
                errors.Add("Employee must be at least 18 years old");
            }
            else if (age > 65)
            {
                errors.Add("Employee must be under 65 years old");
            }

            // Phone number validation
            if (!IsValidPhoneNumber(model.Phone))
            {
                errors.Add("Please enter a valid phone number with country code (e.g., +91-9876543210)");
            }

            if (!IsValidPhoneNumber(model.EmergencyContactPhone))
            {
                errors.Add("Please enter a valid emergency contact phone number");
            }

            // Password strength validation
            var passwordErrors = GetPasswordStrengthErrors(model.Password);
            errors.AddRange(passwordErrors);

            // Address validation
            if (model.Address.Length < 10)
            {
                errors.Add("Address must be at least 10 characters long");
            }

            // Emergency contact validation
            if (model.EmergencyContactName.Trim().Split(' ').Length < 2)
            {
                errors.Add("Emergency contact should include first and last name");
            }

            if (errors.Any())
            {
                return new ValidationResult(string.Join("; ", errors));
            }

            return ValidationResult.Success!;
        }

        public ValidationResult ValidateEmployeeCreation(EmployeeViewModel model)
        {
            var errors = new List<string>();

            // Email validation
            if (!IsValidEmail(model.Email))
            {
                errors.Add("Please enter a valid email address");
            }

            // Salary validation
            if (model.Salary < 100000)
            {
                errors.Add("Minimum salary should be ?1,00,000 per annum");
            }
            else if (model.Salary > 50000000)
            {
                errors.Add("Maximum salary cannot exceed ?5,00,00,000 per annum");
            }

            // Hire date validation
            if (model.HireDate > DateTime.Today)
            {
                errors.Add("Hire date cannot be in the future");
            }
            else if (model.HireDate < DateTime.Today.AddYears(-50))
            {
                errors.Add("Hire date cannot be more than 50 years ago");
            }

            // Department and position validation
            var validDepartments = new[] { "IT", "HR", "Finance", "Sales", "Marketing", "Operations", "Legal", "Admin" };
            if (!validDepartments.Contains(model.Department))
            {
                errors.Add("Please select a valid department");
            }

            if (string.IsNullOrWhiteSpace(model.Position) || model.Position.Length < 3)
            {
                errors.Add("Position must be at least 3 characters long");
            }

            if (errors.Any())
            {
                return new ValidationResult(string.Join("; ", errors));
            }

            return ValidationResult.Success!;
        }

        public ValidationResult ValidateProfileCompletion(Employee employee)
        {
            var missingFields = employee.GetMissingProfileFields();

            if (missingFields.Any())
            {
                return new ValidationResult($"Profile is incomplete. Missing: {string.Join(", ", missingFields)}");
            }

            return ValidationResult.Success!;
        }

        public List<string> GetPasswordStrengthErrors(string password)
        {
            var errors = new List<string>();

            if (password.Length < 8)
            {
                errors.Add("Password must be at least 8 characters long");
            }

            if (!password.Any(char.IsUpper))
            {
                errors.Add("Password must contain at least one uppercase letter");
            }

            if (!password.Any(char.IsLower))
            {
                errors.Add("Password must contain at least one lowercase letter");
            }

            if (!password.Any(char.IsDigit))
            {
                errors.Add("Password must contain at least one number");
            }

            if (!password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c)))
            {
                errors.Add("Password must contain at least one special character");
            }

            // Check for common weak passwords
            var commonPasswords = new[] { "password", "123456", "admin", "qwerty", "letmein" };
            if (commonPasswords.Any(common => password.ToLower().Contains(common)))
            {
                errors.Add("Password contains common words that make it weak");
            }

            return errors;
        }

        public bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Remove spaces, dashes, and parentheses
            var cleanNumber = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            // Check if it starts with + and has proper length
            if (cleanNumber.StartsWith("+"))
            {
                cleanNumber = cleanNumber.Substring(1);
            }

            // Should be 10-15 digits for international numbers
            return cleanNumber.All(char.IsDigit) && cleanNumber.Length >= 10 && cleanNumber.Length <= 15;
        }

        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var emailAttribute = new EmailAddressAttribute();
                return emailAttribute.IsValid(email);
            }
            catch
            {
                return false;
            }
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            var age = DateTime.Today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > DateTime.Today.AddYears(-age))
                age--;
            return age;
        }
    }
}