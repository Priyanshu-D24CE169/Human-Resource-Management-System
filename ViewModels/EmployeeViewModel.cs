using hrms.Models;
using System.ComponentModel.DataAnnotations;

namespace hrms.ViewModels
{
    public class EmployeeProfileViewModel
    {
        public Employee Employee { get; set; } = new Employee();
        public Salary? Salary { get; set; }
        public bool CanEditSalary { get; set; }
        public bool CanEdit { get; set; }
        public string ActiveTab { get; set; } = "resume";
    }

    public class CreateEmployeeViewModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "City")]
        public string? City { get; set; }

        [Display(Name = "State")]
        public string? State { get; set; }

        [Display(Name = "Zip Code")]
        public string? ZipCode { get; set; }

        [Required]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Gender")]
        public string? Gender { get; set; }

        [Display(Name = "Department")]
        public string? Department { get; set; }

        [Display(Name = "Designation")]
        public string? Designation { get; set; }

        [Required]
        [Display(Name = "Joining Date")]
        [DataType(DataType.Date)]
        public DateTime JoiningDate { get; set; } = DateTime.Now;

        [Display(Name = "Emergency Contact Name")]
        public string? EmergencyContactName { get; set; }

        [Display(Name = "Emergency Contact Phone")]
        public string? EmergencyContactPhone { get; set; }

        [Display(Name = "Bank Name")]
        public string? BankName { get; set; }

        [Display(Name = "Account Number")]
        public string? AccountNumber { get; set; }

        [Display(Name = "IFSC Code")]
        public string? IFSCCode { get; set; }

        [Display(Name = "PAN Number")]
        public string? PANNumber { get; set; }

        [Display(Name = "Aadhar Number")]
        public string? AadharNumber { get; set; }

        [Display(Name = "Profile Photo")]
        public IFormFile? ProfilePhoto { get; set; }

        [Display(Name = "Resume")]
        public IFormFile? Resume { get; set; }

        [Required]
        [Display(Name = "Monthly Salary")]
        public decimal MonthlySalary { get; set; }
    }
}
