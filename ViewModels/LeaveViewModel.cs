using hrms.Models;
using System.ComponentModel.DataAnnotations;

namespace hrms.ViewModels
{
    public class LeaveRequestViewModel
    {
        public List<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        public bool IsAdmin { get; set; }
        public Employee? CurrentEmployee { get; set; }
    }

    public class CreateLeaveRequestViewModel
    {
        [Required]
        [Display(Name = "From Date")]
        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "To Date")]
        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Leave Type")]
        public string LeaveType { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [Display(Name = "Reason")]
        public string Reason { get; set; } = string.Empty;

        [Display(Name = "Attachment (Optional)")]
        public IFormFile? Attachment { get; set; }

        public Employee? Employee { get; set; }
    }
}
