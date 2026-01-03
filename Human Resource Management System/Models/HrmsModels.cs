using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Human_Resource_Management_System.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }
        
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
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }
        
        // Navigation properties
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
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