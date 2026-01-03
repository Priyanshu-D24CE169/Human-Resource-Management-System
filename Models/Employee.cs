using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hrms.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string LoginId { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(15)]
        public string? PhoneNumber { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? State { get; set; }

        [StringLength(10)]
        public string? ZipCode { get; set; }

        public DateTime DateOfBirth { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(100)]
        public string? Designation { get; set; }

        public DateTime JoiningDate { get; set; }

        [StringLength(10)]
        public string CompanyCode { get; set; } = string.Empty;

        public int YearOfJoining { get; set; }

        public int SerialNumber { get; set; }

        [StringLength(255)]
        public string? ProfilePhotoPath { get; set; }

        [StringLength(255)]
        public string? ResumePath { get; set; }

        [StringLength(20)]
        public string? EmergencyContactName { get; set; }

        [StringLength(15)]
        public string? EmergencyContactPhone { get; set; }

        [StringLength(100)]
        public string? BankName { get; set; }

        [StringLength(50)]
        public string? AccountNumber { get; set; }

        [StringLength(20)]
        public string? IFSCCode { get; set; }

        [StringLength(20)]
        public string? PANNumber { get; set; }

        [StringLength(20)]
        public string? AadharNumber { get; set; }

        // Leave Balance Properties
        public int PaidTimeOffBalance { get; set; } = 24; // Annual PTO allocation
        public int SickTimeOffBalance { get; set; } = 12; // Annual sick leave allocation
        public int CurrentYearPTOUsed { get; set; } = 0; // PTO used this year
        public int CurrentYearSickUsed { get; set; } = 0; // Sick leave used this year
        public DateTime LeaveBalanceLastReset { get; set; } = DateTime.Now; // Track when balances were last reset

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        public Salary? Salary { get; set; }

        // Computed Properties for Available Leave
        [NotMapped]
        public int AvailablePTO => PaidTimeOffBalance - CurrentYearPTOUsed;

        [NotMapped]
        public int AvailableSickDays => SickTimeOffBalance - CurrentYearSickUsed;

        [NotMapped]
        public string PTOStatus => $"{AvailablePTO} Days Available";

        [NotMapped]
        public string SickTimeStatus => $"{AvailableSickDays:D2} Days Available";
    }
}
