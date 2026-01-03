using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hrms.Models
{
    public class LeaveRequest
    {
        [Key]
        public int LeaveRequestId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!;

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        [Required]
        [StringLength(50)]
        public string LeaveType { get; set; } = string.Empty; // PaidLeave, SickLeave, UnpaidLeave

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(255)]
        public string? AttachmentPath { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        [StringLength(500)]
        public string? AdminRemarks { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.Now;

        public DateTime? ApprovedDate { get; set; }

        public string? ApprovedBy { get; set; }

        public int TotalDays { get; set; }
    }
}
