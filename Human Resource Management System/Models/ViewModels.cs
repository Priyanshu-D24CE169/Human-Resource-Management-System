using System.ComponentModel.DataAnnotations;

namespace Human_Resource_Management_System.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class EmployeeViewModel
    {
        public int EmployeeId { get; set; }
        
        public string EmployeeCode { get; set; } = string.Empty;
        
        // For backward compatibility with existing views
        public string Id 
        { 
            get => string.IsNullOrEmpty(EmployeeCode) ? $"EMP{EmployeeId:000}" : EmployeeCode;
            set { } // Allow setting but ignore it
        }
        
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = string.Empty;
        
        // For backward compatibility with existing views
        public string Name 
        { 
            get => $"{FirstName} {LastName}".Trim();
            set 
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0)
                        FirstName = parts[0];
                    if (parts.Length > 1)
                        LastName = string.Join(" ", parts.Skip(1));
                }
            }
        }
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
        
        [MaxLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
        public string Phone { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Department is required")]
        public string Department { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Position is required")]
        public string Position { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Hire date is required")]
        public DateTime HireDate { get; set; } = DateTime.Today;
        
        // For backward compatibility with existing views
        public DateTime JoinDate 
        { 
            get => HireDate;
            set => HireDate = value;
        }
        
        [Required(ErrorMessage = "Salary is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive number")]
        public decimal Salary { get; set; }
        
        public string Status { get; set; } = "Active";
        
        // Computed property for display
        public string FullName => $"{FirstName} {LastName}".Trim();
    }

    public class EmployeeRegistrationViewModel
    {
        [Required(ErrorMessage = "Registration token is required")]
        public string Token { get; set; } = string.Empty;
        
        public string EmployeeCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [RegularExpression(@"^\+?[1-9]\d{1,14}$", ErrorMessage = "Please enter a valid phone number with country code")]
        public string Phone { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Address is required")]
        [MaxLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        [MinLength(10, ErrorMessage = "Address must be at least 10 characters long")]
        public string Address { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Emergency contact name is required")]
        [MaxLength(100, ErrorMessage = "Emergency contact name cannot exceed 100 characters")]
        [MinLength(2, ErrorMessage = "Emergency contact name must be at least 2 characters long")]
        public string EmergencyContactName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Emergency contact phone is required")]
        [Phone(ErrorMessage = "Invalid emergency contact phone format")]
        [RegularExpression(@"^\+?[1-9]\d{1,14}$", ErrorMessage = "Please enter a valid emergency contact phone number")]
        public string EmergencyContactPhone { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(EmployeeRegistrationViewModel), nameof(ValidateAge))]
        public DateTime DateOfBirth { get; set; }
        
        [Required(ErrorMessage = "Gender is required")]
        [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Please select a valid gender option")]
        public string Gender { get; set; } = string.Empty;
        
        [MaxLength(50, ErrorMessage = "Nationality cannot exceed 50 characters")]
        public string? Nationality { get; set; } = "Indian";
        
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Password confirmation is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
        
        // Profile image upload
        [DataType(DataType.Upload)]
        [FileExtensions(Extensions = "jpg,jpeg,png,gif", ErrorMessage = "Please upload a valid image file (JPG, JPEG, PNG, GIF)")]
        public IFormFile? ProfileImage { get; set; }
        
        // Custom validation for age
        public static ValidationResult? ValidateAge(DateTime dateOfBirth, ValidationContext context)
        {
            var age = DateTime.Today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;
            
            if (age < 18)
            {
                return new ValidationResult("Employee must be at least 18 years old");
            }
            
            if (age > 65)
            {
                return new ValidationResult("Employee must be under 65 years old");
            }
            
            return ValidationResult.Success;
        }
    }

    public class AttendanceViewModel
    {
        public int AttendanceId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public TimeSpan CheckIn { get; set; }
        public TimeSpan CheckOut { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal WorkingHours { get; set; }
    }

    public class PayrollViewModel
    {
        public int PayrollId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime PayPeriodStart { get; set; }
        public DateTime PayPeriodEnd { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal Allowances { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
        public DateTime PayDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class EmployeeDashboardViewModel
    {
        public Employee Employee { get; set; } = new Employee();
        public double ProfileCompletionPercentage { get; set; }
        public List<string> MissingFields { get; set; } = new List<string>();
        public bool IsProfileComplete { get; set; }
        public int DaysWithCompany { get; set; }
        public DateTime? NextBirthday { get; set; }
        public double YearsOfService { get; set; }
        public int DaysUntilBirthday => NextBirthday.HasValue ? (NextBirthday.Value - DateTime.Today).Days : 0;
    }

    public class EmployeeProfileEditViewModel
    {
        public int EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string Phone { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Address is required")]
        [MaxLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string Address { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Emergency contact name is required")]
        [MaxLength(100, ErrorMessage = "Emergency contact name cannot exceed 100 characters")]
        public string EmergencyContactName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Emergency contact phone is required")]
        [Phone(ErrorMessage = "Invalid emergency contact phone format")]
        public string EmergencyContactPhone { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        
        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = string.Empty;
        
        public string? Nationality { get; set; } = "Indian";
        
        // Profile image upload
        public IFormFile? ProfileImage { get; set; }
    }

    // Enhanced User Registration ViewModel with Complete Profile
    public class UserRegistrationViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required")]
        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "ZIP code is required")]
        [StringLength(10, ErrorMessage = "ZIP code cannot exceed 10 characters")]
        [Display(Name = "ZIP Code")]
        public string ZipCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Position is required")]
        [StringLength(100, ErrorMessage = "Position cannot exceed 100 characters")]
        public string Position { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hire date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Hire Date")]
        public DateTime HireDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Salary is required")]
        [Range(1, 10000000, ErrorMessage = "Salary must be between 1 and 10,000,000")]
        [Display(Name = "Annual Salary")]
        public decimal Salary { get; set; }

        [StringLength(100, ErrorMessage = "Emergency contact name cannot exceed 100 characters")]
        [Display(Name = "Emergency Contact Name")]
        public string? EmergencyContactName { get; set; }

        [Phone(ErrorMessage = "Please enter a valid emergency contact phone")]
        [StringLength(20, ErrorMessage = "Emergency contact phone cannot exceed 20 characters")]
        [Display(Name = "Emergency Contact Phone")]
        public string? EmergencyContactPhone { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password confirmation is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "You must agree to the terms and conditions")]
        [Display(Name = "I agree to the Terms and Conditions")]
        public bool AgreeToTerms { get; set; }

        [Display(Name = "Subscribe to newsletters")]
        public bool SubscribeToNewsletter { get; set; }

        // Role selection for admin users
        public string Role { get; set; } = "Employee";

        // Status for admin control
        public string Status { get; set; } = "Active";

        // Profile picture path
        [Display(Name = "Profile Picture")]
        public string? ProfilePicturePath { get; set; }

        // Additional fields for complete profile
        [StringLength(50, ErrorMessage = "Nationality cannot exceed 50 characters")]
        public string? Nationality { get; set; } = "Indian";

        [StringLength(20, ErrorMessage = "Employee ID cannot exceed 20 characters")]
        [Display(Name = "Employee ID")]
        public string? EmployeeId { get; set; }

        // Calculated properties
        public string FullName => $"{FirstName} {LastName}";
        public string FullAddress => $"{Address}, {City}, {State} {ZipCode}";
        
        // Age validation
        public int Age => DateTime.Today.Year - DateOfBirth.Year - 
                         (DateTime.Today.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
    }

    // User Registration Confirmation Model
    public class UserRegistrationConfirmationViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? ProfilePicturePath { get; set; }
    }
}