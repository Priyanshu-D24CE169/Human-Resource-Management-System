using Microsoft.AspNetCore.Mvc;
using Human_Resource_Management_System.Models;

namespace Human_Resource_Management_System.Controllers
{
    public class AttendanceController : BaseController
    {
        public IActionResult Index()
        {
            // Sample data - in real application, this would come from database
            var attendance = new List<AttendanceViewModel>
            {
                new AttendanceViewModel { AttendanceId = 1, EmployeeId = 1, EmployeeName = "John Doe", Date = DateTime.Today, CheckIn = new TimeSpan(9, 15, 0), CheckOut = new TimeSpan(18, 30, 0), Status = "Present", WorkingHours = 8.25m },
                new AttendanceViewModel { AttendanceId = 2, EmployeeId = 2, EmployeeName = "Jane Smith", Date = DateTime.Today, CheckIn = new TimeSpan(8, 45, 0), CheckOut = new TimeSpan(17, 45, 0), Status = "Present", WorkingHours = 9.0m },
                new AttendanceViewModel { AttendanceId = 3, EmployeeId = 3, EmployeeName = "Mike Johnson", Date = DateTime.Today, CheckIn = new TimeSpan(0, 0, 0), CheckOut = new TimeSpan(0, 0, 0), Status = "Absent", WorkingHours = 0 },
                new AttendanceViewModel { AttendanceId = 4, EmployeeId = 4, EmployeeName = "Sarah Wilson", Date = DateTime.Today, CheckIn = new TimeSpan(9, 0, 0), CheckOut = new TimeSpan(18, 0, 0), Status = "Present", WorkingHours = 8.0m },
                new AttendanceViewModel { AttendanceId = 5, EmployeeId = 5, EmployeeName = "Robert Brown", Date = DateTime.Today, CheckIn = new TimeSpan(0, 0, 0), CheckOut = new TimeSpan(0, 0, 0), Status = "Leave", WorkingHours = 0 }
            };
            
            return View(attendance);
        }
    }
}