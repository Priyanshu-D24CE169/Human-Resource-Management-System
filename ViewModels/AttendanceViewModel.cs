using hrms.Models;

namespace hrms.ViewModels
{
    public class AttendanceViewModel
    {
        public List<Attendance> Attendances { get; set; } = new List<Attendance>();
        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public bool CanCheckIn { get; set; }
        public bool CanCheckOut { get; set; }
        public Attendance? TodayAttendance { get; set; }
        public int EmployeeId { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class AttendanceReportViewModel
    {
        public List<Attendance> Attendances { get; set; } = new List<Attendance>();
        public DateTime FromDate { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime ToDate { get; set; } = DateTime.Today;
        public int? EmployeeId { get; set; }
        public List<Employee> Employees { get; set; } = new List<Employee>();
    }
}
