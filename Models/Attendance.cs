using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hrms.Models
{
    public class Attendance
    {
        [Key]
        public int AttendanceId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; } = null!;

        [Required]
        public DateTime Date { get; set; }

        public DateTime? CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal WorkHours { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal ExtraHours { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Absent"; // Present, Absent, OnLeave

        [StringLength(500)]
        public string? Remarks { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
