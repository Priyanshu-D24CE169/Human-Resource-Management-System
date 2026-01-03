using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Human_Resource_Management_System.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }
        
        // Auto-generated Employee Code in Odoo India HRMS format: OITODO20230001
        [Required]
        [MaxLength(20)]
        public string EmployeeCode { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Department { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Position { get; set; } = string.Empty;
        
        public DateTime HireDate { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }
        
        [MaxLength(20)]
        public string Status { get; set; } = "Active";
        
        // Employee registration fields
        public bool IsRegistrationComplete { get; set; } = false;
        public string? RegistrationToken { get; set; } = string.Empty;
        public DateTime? RegistrationTokenExpiry { get; set; }
        
        // Additional employee profile fields
        [MaxLength(200)]
        public string? Address { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string? EmergencyContactName { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string? EmergencyContactPhone { get; set; } = string.Empty;
        
        public DateTime? DateOfBirth { get; set; }
        
        [MaxLength(10)]
        public string? Gender { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string? Nationality { get; set; } = string.Empty;
        
        // Profile image
        public string? ProfileImagePath { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }
        
        // Navigation properties
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
        
        // Full name property for display
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
        
        // Method to generate employee code
        public static string GenerateEmployeeCode(string firstName, string lastName, DateTime hireDate, int serialNumber)
        {
            // Format: OI + FirstTwoLettersFirstName + FirstTwoLettersLastName + Year + SerialNumber(4 digits)
            // Example: OITODO20230001 (Odoo India + TODO + 2023 + 0001)
            
            var firstInitials = firstName.Length >= 2 ? firstName.Substring(0, 2).ToUpper() : firstName.ToUpper().PadRight(2, 'X');
            var lastInitials = lastName.Length >= 2 ? lastName.Substring(0, 2).ToUpper() : lastName.ToUpper().PadRight(2, 'X');
            var year = hireDate.Year.ToString();
            var serial = serialNumber.ToString("D4");
            
            return $"OI{firstInitials}{lastInitials}{year}{serial}";
        }
        
        // Method to check if profile is complete
        public bool IsProfileComplete()
        {
            return !string.IsNullOrEmpty(Phone) &&
                   !string.IsNullOrEmpty(Address) &&
                   !string.IsNullOrEmpty(EmergencyContactName) &&
                   !string.IsNullOrEmpty(EmergencyContactPhone) &&
                   DateOfBirth.HasValue &&
                   !string.IsNullOrEmpty(Gender) &&
                   IsRegistrationComplete;
        }
        
        // Method to get missing profile fields
        public List<string> GetMissingProfileFields()
        {
            var missingFields = new List<string>();
            
            if (string.IsNullOrEmpty(Phone))
                missingFields.Add("Phone Number");
            if (string.IsNullOrEmpty(Address))
                missingFields.Add("Address");
            if (string.IsNullOrEmpty(EmergencyContactName))
                missingFields.Add("Emergency Contact Name");
            if (string.IsNullOrEmpty(EmergencyContactPhone))
                missingFields.Add("Emergency Contact Phone");
            if (!DateOfBirth.HasValue)
                missingFields.Add("Date of Birth");
            if (string.IsNullOrEmpty(Gender))
                missingFields.Add("Gender");
            if (!IsRegistrationComplete)
                missingFields.Add("Registration Completion");
                
            return missingFields;
        }
        
        // Method to calculate profile completion percentage
        public double GetProfileCompletionPercentage()
        {
            var requiredFields = 7; // Basic + Phone, Address, Emergency contacts, DOB, Gender, Registration
            var completedFields = 2; // FirstName and LastName are always required
            
            if (!string.IsNullOrEmpty(Phone)) completedFields++;
            if (!string.IsNullOrEmpty(Address)) completedFields++;
            if (!string.IsNullOrEmpty(EmergencyContactName)) completedFields++;
            if (!string.IsNullOrEmpty(EmergencyContactPhone)) completedFields++;
            if (DateOfBirth.HasValue) completedFields++;
            if (!string.IsNullOrEmpty(Gender)) completedFields++;
            if (IsRegistrationComplete) completedFields++;
            
            return Math.Round((double)completedFields / (requiredFields + 2) * 100, 0);
        }
    }

    public class Attendance
    {
        [Key]
        public int AttendanceId { get; set; }
        
        [Required]
        public int EmployeeId { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        public TimeSpan? CheckIn { get; set; }
        public TimeSpan? CheckOut { get; set; }
        
        [MaxLength(20)]
        public string Status { get; set; } = "Present"; // Present, Absent, Leave, Half-Day
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal WorkingHours { get; set; }
        
        public string? Notes { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Navigation property
        public virtual Employee Employee { get; set; } = null!;
    }

    public class Payroll
    {
        [Key]
        public int PayrollId { get; set; }
        
        [Required]
        public int EmployeeId { get; set; }
        
        public DateTime PayPeriodStart { get; set; }
        public DateTime PayPeriodEnd { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal BasicSalary { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Allowances { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Deductions { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetSalary { get; set; }
        
        public DateTime PayDate { get; set; }
        
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Processing, Paid
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Navigation property
        public virtual Employee Employee { get; set; } = null!;
    }

    public class LeaveRequest
    {
        [Key]
        public int LeaveRequestId { get; set; }
        
        [Required]
        public int EmployeeId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string LeaveType { get; set; } = string.Empty; // Sick, Vacation, Personal, etc.
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public string? Reason { get; set; }
        
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public DateTime? ApprovalDate { get; set; }
        
        public string? ApproverComments { get; set; }
        
        // Navigation property
        public virtual Employee Employee { get; set; } = null!;
    }
}