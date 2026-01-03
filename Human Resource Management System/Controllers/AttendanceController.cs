using Microsoft.AspNetCore.Mvc;
using Human_Resource_Management_System.Models;
using Human_Resource_Management_System.Services;
using Human_Resource_Management_System.Attributes;

namespace Human_Resource_Management_System.Controllers
{
    [AdminAuthorize]
    public class AttendanceController : BaseController
    {
        private readonly IAttendanceService _attendanceService;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(IAttendanceService attendanceService, ILogger<AttendanceController> logger)
        {
            _attendanceService = attendanceService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(DateTime? date, string? department, string? status)
        {
            try
            {
                var selectedDate = date ?? DateTime.Today;
                var attendance = await _attendanceService.GetAttendanceByDateAsync(selectedDate);
                var summary = await _attendanceService.GetAttendanceSummaryAsync(selectedDate);
                var employees = await _attendanceService.GetAllEmployeesAsync();

                // Apply filters
                if (!string.IsNullOrEmpty(department))
                {
                    attendance = attendance.Where(a => a.Department == department).ToList();
                }
                if (!string.IsNullOrEmpty(status))
                {
                    attendance = attendance.Where(a => a.Status == status).ToList();
                }

                ViewBag.SelectedDate = selectedDate;
                ViewBag.Summary = summary;
                ViewBag.Employees = employees;
                ViewBag.DepartmentFilter = department;
                ViewBag.StatusFilter = status;

                return View(attendance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading attendance list");
                TempData["Error"] = "Error loading attendance. Please try again.";
                return View(new List<AttendanceViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAttendance([FromBody] MarkAttendanceViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var success = await _attendanceService.MarkAttendanceAsync(
                    model.EmployeeId,
                    model.Date,
                    model.CheckIn,
                    model.CheckOut,
                    model.Status,
                    model.Notes
                );

                if (success)
                {
                    return Json(new { success = true, message = "Attendance marked successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to mark attendance." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking attendance for employee {model.EmployeeId}");
                return Json(new { success = false, message = "An error occurred while marking attendance." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAttendance(int id)
        {
            try
            {
                var attendance = await _attendanceService.GetAttendanceByIdAsync(id);
                if (attendance == null)
                {
                    return Json(new { success = false, message = "Attendance record not found." });
                }

                return Json(new { 
                    success = true, 
                    data = new {
                        attendanceId = attendance.AttendanceId,
                        employeeId = attendance.EmployeeId,
                        employeeName = attendance.EmployeeName,
                        date = attendance.Date.ToString("yyyy-MM-dd"),
                        checkIn = attendance.CheckIn == TimeSpan.Zero ? "" : attendance.CheckIn.ToString(@"hh\:mm"),
                        checkOut = attendance.CheckOut == TimeSpan.Zero ? "" : attendance.CheckOut.ToString(@"hh\:mm"),
                        status = attendance.Status,
                        notes = attendance.Notes,
                        workingHours = attendance.WorkingHours
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting attendance: {id}");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAttendance([FromBody] UpdateAttendanceRequest model)
        {
            try
            {
                TimeSpan? checkIn = null;
                TimeSpan? checkOut = null;

                if (!string.IsNullOrEmpty(model.CheckIn) && TimeSpan.TryParse(model.CheckIn, out var parsedCheckIn))
                {
                    checkIn = parsedCheckIn;
                }
                if (!string.IsNullOrEmpty(model.CheckOut) && TimeSpan.TryParse(model.CheckOut, out var parsedCheckOut))
                {
                    checkOut = parsedCheckOut;
                }

                var success = await _attendanceService.UpdateAttendanceAsync(
                    model.AttendanceId,
                    checkIn,
                    checkOut,
                    model.Status,
                    model.Notes
                );

                if (success)
                {
                    return Json(new { success = true, message = "Attendance updated successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update attendance." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating attendance: {model.AttendanceId}");
                return Json(new { success = false, message = "An error occurred while updating attendance." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            try
            {
                var success = await _attendanceService.DeleteAttendanceAsync(id);
                if (success)
                {
                    return Json(new { success = true, message = "Attendance record deleted successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to delete attendance record." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting attendance: {id}");
                return Json(new { success = false, message = "An error occurred while deleting attendance." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> EmployeeAttendance(int id)
        {
            try
            {
                var attendance = await _attendanceService.GetAttendanceByEmployeeAsync(id);
                return Json(new { success = true, data = attendance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting attendance for employee: {id}");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportReport(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddMonths(-1);
                var end = endDate ?? DateTime.Today;
                
                var attendance = await _attendanceService.GetAttendanceByDateRangeAsync(start, end);
                
                // Generate CSV content
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("Employee Code,Employee Name,Department,Date,Check In,Check Out,Working Hours,Status,Notes");
                
                foreach (var record in attendance)
                {
                    csv.AppendLine($"{record.EmployeeCode},{record.EmployeeName},{record.Department},{record.Date:yyyy-MM-dd},{record.CheckIn:hh\\:mm},{record.CheckOut:hh\\:mm},{record.WorkingHours},{record.Status},{record.Notes?.Replace(",", ";")}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"Attendance_Report_{start:yyyyMMdd}_{end:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting attendance report");
                TempData["Error"] = "Error exporting report. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSummary(DateTime date)
        {
            try
            {
                var summary = await _attendanceService.GetAttendanceSummaryAsync(date);
                return Json(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting summary for date: {date}");
                return Json(new { success = false, message = "An error occurred." });
            }
        }
    }

    public class UpdateAttendanceRequest
    {
        public int AttendanceId { get; set; }
        public string? CheckIn { get; set; }
        public string? CheckOut { get; set; }
        public string Status { get; set; } = "Present";
        public string? Notes { get; set; }
    }
}