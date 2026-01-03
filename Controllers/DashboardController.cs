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
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
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
            var today = DateTime.Today;

            var viewModel = new DashboardViewModel
            {
                CurrentUserRole = isAdmin ? "Admin" : "Employee",
                CurrentEmployeeId = currentUser?.EmployeeId
            };

            // Get current user's attendance status for header systray
            if (currentUser?.EmployeeId != null)
            {
                var currentUserAttendance = await _context.Attendances
                    .Where(a => a.EmployeeId == currentUser.EmployeeId && a.Date.Date == today)
                    .FirstOrDefaultAsync();

                viewModel.CurrentUserCheckInStatus = new UserCheckInStatus
                {
                    HasCheckedIn = currentUserAttendance?.CheckInTime != null,
                    CheckInTime = currentUserAttendance?.CheckInTime,
                    CheckOutTime = currentUserAttendance?.CheckOutTime,
                    CanCheckOut = currentUserAttendance?.CheckInTime != null && currentUserAttendance?.CheckOutTime == null
                };
            }

            IQueryable<Employee> employeesQuery = _context.Employees
                .Where(e => e.IsActive);

            if (!isAdmin && currentUser?.EmployeeId != null)
            {
                employeesQuery = employeesQuery.Where(e => e.EmployeeId == currentUser.EmployeeId);
            }

            var employees = await employeesQuery.ToListAsync();

            foreach (var employee in employees)
            {
                // Priority 1: Check for approved leave (?? Airplane Icon)
                var todayLeave = await _context.LeaveRequests
                    .Where(l => l.EmployeeId == employee.EmployeeId 
                        && l.Status == "Approved"
                        && l.FromDate.Date <= today 
                        && l.ToDate.Date >= today)
                    .AnyAsync();

                // Priority 2: Check for attendance check-in (?? Green Dot)
                var todayAttendance = await _context.Attendances
                    .Where(a => a.EmployeeId == employee.EmployeeId && a.Date.Date == today)
                    .FirstOrDefaultAsync();

                string status;
                string statusIcon;
                string statusColor;
                string statusTooltip;

                if (todayLeave)
                {
                    // Priority 1: On Leave
                    status = "OnLeave";
                    statusIcon = "airplane";
                    statusColor = "primary";
                    statusTooltip = "On Leave";
                }
                else if (todayAttendance != null && todayAttendance.CheckInTime != null)
                {
                    // Priority 2: Present
                    status = "Present";
                    statusIcon = "circle-fill";
                    statusColor = "success";
                    statusTooltip = "Present";
                }
                else
                {
                    // Priority 3: Absent (Default/Fallback)
                    status = "Absent";
                    statusIcon = "circle-fill";
                    statusColor = "warning";
                    statusTooltip = "Absent";
                }

                viewModel.Employees.Add(new EmployeeCardViewModel
                {
                    EmployeeId = employee.EmployeeId,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    ProfilePhotoPath = employee.ProfilePhotoPath,
                    Designation = employee.Designation,
                    Department = employee.Department,
                    JoiningDate = employee.JoiningDate,
                    Status = status,
                    StatusIcon = statusIcon,
                    StatusColor = statusColor,
                    StatusTooltip = statusTooltip
                });
            }

            return View(viewModel);
        }
    }
}
