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
        
        // For backward compatibility with existing views
        public string Id 
        { 
            get => $"EMP{EmployeeId:000}";
            set { } // Allow setting but ignore it
        }
        
        public string FirstName { get; set; } = string.Empty;
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
        
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        
        public DateTime HireDate { get; set; }
        
        // For backward compatibility with existing views
        public DateTime JoinDate 
        { 
            get => HireDate;
            set => HireDate = value;
        }
        
        public decimal Salary { get; set; }
        public string Status { get; set; } = "Active";
        
        // Computed property for display
        public string FullName => $"{FirstName} {LastName}".Trim();
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
}