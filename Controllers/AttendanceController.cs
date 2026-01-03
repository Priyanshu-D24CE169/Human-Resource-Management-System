using hrms.Data;
using hrms.Models;
using hrms.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hrms.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AttendanceController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser!);
            var isAdmin = roles.Contains("Admin");

            if (!isAdmin && currentUser?.EmployeeId == null)
            {
                return Forbid();
            }

            var employeeId = isAdmin ? 0 : currentUser!.EmployeeId!.Value;
            var today = DateTime.Today;

            var todayAttendance = employeeId > 0 
                ? await _context.Attendances
                    .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date.Date == today)
                : null;

            var canCheckIn = !isAdmin && employeeId > 0 && 
                (todayAttendance == null || todayAttendance.CheckInTime == null);
            
            var canCheckOut = !isAdmin && employeeId > 0 && 
                todayAttendance != null && 
                todayAttendance.CheckInTime != null && 
                todayAttendance.CheckOutTime == null;

            IQueryable<Attendance> attendancesQuery = _context.Attendances
                .Include(a => a.Employee);

            if (!isAdmin)
            {
                attendancesQuery = attendancesQuery.Where(a => a.EmployeeId == employeeId);
            }

            var attendances = await attendancesQuery
                .OrderByDescending(a => a.Date)
                .Take(30)
                .ToListAsync();

            var viewModel = new AttendanceViewModel
            {
                Attendances = attendances,
                SelectedDate = today,
                CanCheckIn = canCheckIn,
                CanCheckOut = canCheckOut,
                TodayAttendance = todayAttendance,
                EmployeeId = employeeId,
                IsAdmin = isAdmin
            };

            return View(viewModel);
        }

        // API endpoint for header systray status
        [HttpGet]
        [Route("api/attendance/status")]
        public async Task<IActionResult> GetAttendanceStatus()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser?.EmployeeId == null)
                {
                    return Json(new { error = true, message = "Employee not found" });
                }

                var employeeId = currentUser.EmployeeId.Value;
                var today = DateTime.Today;

                var todayAttendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date.Date == today);

                var hasCheckedIn = todayAttendance?.CheckInTime != null;
                var canCheckOut = hasCheckedIn && todayAttendance?.CheckOutTime == null;

                return Json(new
                {
                    hasCheckedIn,
                    checkInTime = todayAttendance?.CheckInTime,
                    checkOutTime = todayAttendance?.CheckOutTime,
                    canCheckOut,
                    workHours = todayAttendance?.WorkHours,
                    status = hasCheckedIn ? "Present" : "Not Checked In"
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, message = "Failed to load status" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser?.EmployeeId == null)
                {
                    if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                    {
                        return Json(new { success = false, message = "Employee not found" });
                    }
                    return BadRequest("Employee not found");
                }

                var employeeId = currentUser.EmployeeId.Value;
                var today = DateTime.Today;
                var now = DateTime.Now;

                var existingAttendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date.Date == today);

                if (existingAttendance != null && existingAttendance.CheckInTime != null)
                {
                    var message = "You have already checked in today!";
                    if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                    {
                        return Json(new { success = false, message });
                    }
                    TempData["ErrorMessage"] = message;
                    return RedirectToAction(nameof(Index));
                }

                if (existingAttendance == null)
                {
                    existingAttendance = new Attendance
                    {
                        EmployeeId = employeeId,
                        Date = today,
                        CheckInTime = now,
                        Status = "Present",
                        CreatedDate = DateTime.Now
                    };
                    _context.Attendances.Add(existingAttendance);
                }
                else
                {
                    existingAttendance.CheckInTime = now;
                    existingAttendance.Status = "Present";
                }

                await _context.SaveChangesAsync();

                var successMessage = $"Checked in successfully at {now:hh:mm tt}";
                if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                {
                    return Json(new { success = true, message = successMessage, checkInTime = now });
                }
                
                TempData["SuccessMessage"] = successMessage;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var errorMessage = "Check in failed. Please try again.";
                if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = errorMessage });
                }
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser?.EmployeeId == null)
                {
                    if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                    {
                        return Json(new { success = false, message = "Employee not found" });
                    }
                    return BadRequest("Employee not found");
                }

                var employeeId = currentUser.EmployeeId.Value;
                var today = DateTime.Today;
                var now = DateTime.Now;

                var attendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date.Date == today);

                if (attendance == null || attendance.CheckInTime == null)
                {
                    var message = "Please check in first!";
                    if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                    {
                        return Json(new { success = false, message });
                    }
                    TempData["ErrorMessage"] = message;
                    return RedirectToAction(nameof(Index));
                }

                if (attendance.CheckOutTime != null)
                {
                    var message = "You have already checked out today!";
                    if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                    {
                        return Json(new { success = false, message });
                    }
                    TempData["ErrorMessage"] = message;
                    return RedirectToAction(nameof(Index));
                }

                attendance.CheckOutTime = now;

                var workHours = (now - attendance.CheckInTime.Value).TotalHours;
                attendance.WorkHours = (decimal)workHours;

                const decimal standardHours = 8.0m;
                if (attendance.WorkHours > standardHours)
                {
                    attendance.ExtraHours = attendance.WorkHours - standardHours;
                }

                _context.Attendances.Update(attendance);
                await _context.SaveChangesAsync();

                var workHoursText = $"{Math.Floor(workHours)}h {(int)((workHours % 1) * 60)}m";
                var successMessage = $"Checked out successfully at {now:hh:mm tt}";
                
                if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                {
                    return Json(new { 
                        success = true, 
                        message = successMessage,
                        workingHours = workHoursText,
                        checkOutTime = now
                    });
                }
                
                TempData["SuccessMessage"] = $"{successMessage}. Work hours: {attendance.WorkHours:F2}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var errorMessage = "Check out failed. Please try again.";
                if (Request.Headers["Content-Type"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = errorMessage });
                }
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Report(DateTime? fromDate, DateTime? toDate, int? employeeId)
        {
            var from = fromDate ?? DateTime.Today.AddDays(-30);
            var to = toDate ?? DateTime.Today;

            var attendancesQuery = _context.Attendances
                .Include(a => a.Employee)
                .Where(a => a.Date.Date >= from.Date && a.Date.Date <= to.Date);

            if (employeeId.HasValue)
            {
                attendancesQuery = attendancesQuery.Where(a => a.EmployeeId == employeeId.Value);
            }

            var attendances = await attendancesQuery
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.Employee.FirstName)
                .ToListAsync();

            var employees = await _context.Employees
                .Where(e => e.IsActive)
                .OrderBy(e => e.FirstName)
                .ToListAsync();

            var viewModel = new AttendanceReportViewModel
            {
                Attendances = attendances,
                FromDate = from,
                ToDate = to,
                EmployeeId = employeeId,
                Employees = employees
            };

            return View(viewModel);
        }
    }
}
