using Microsoft.AspNetCore.Mvc;
using Human_Resource_Management_System.Models;
using Human_Resource_Management_System.Services;

namespace Human_Resource_Management_System.Controllers
{
    public class EmployeeDashboardController : BaseController
    {
        private readonly IEmployeeService _employeeService;
        private readonly IUserService _userService;
        private readonly ILogger<EmployeeDashboardController> _logger;

        public EmployeeDashboardController(
            IEmployeeService employeeService, 
            IUserService userService, 
            ILogger<EmployeeDashboardController> logger)
        {
            _employeeService = employeeService;
            _userService = userService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userEmail = HttpContext.Session.GetString("UserEmail");
                if (string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToAction("Index", "Login");
                }

                var employee = await _employeeService.GetEmployeeByEmailAsync(userEmail);
                if (employee == null)
                {
                    TempData["Error"] = "Employee profile not found.";
                    return RedirectToAction("Index", "Login");
                }

                var dashboardModel = new EmployeeDashboardViewModel
                {
                    Employee = employee,
                    ProfileCompletionPercentage = employee.GetProfileCompletionPercentage(),
                    MissingFields = employee.GetMissingProfileFields(),
                    IsProfileComplete = employee.IsProfileComplete(),
                    DaysWithCompany = (DateTime.Now - employee.HireDate).Days,
                    NextBirthday = GetNextBirthday(employee.DateOfBirth),
                    YearsOfService = GetYearsOfService(employee.HireDate)
                };

                return View(dashboardModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading employee dashboard for user: {HttpContext.Session.GetString("UserEmail")}");
                TempData["Error"] = "Error loading dashboard. Please try again.";
                return RedirectToAction("Index", "Login");
            }
        }

        public async Task<IActionResult> Profile()
        {
            try
            {
                var userEmail = HttpContext.Session.GetString("UserEmail");
                if (string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToAction("Index", "Login");
                }

                var employee = await _employeeService.GetEmployeeByEmailAsync(userEmail);
                if (employee == null)
                {
                    TempData["Error"] = "Employee profile not found.";
                    return RedirectToAction("Index", "Login");
                }

                return View(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading employee profile for user: {HttpContext.Session.GetString("UserEmail")}");
                TempData["Error"] = "Error loading profile. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            try
            {
                var userEmail = HttpContext.Session.GetString("UserEmail");
                if (string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToAction("Index", "Login");
                }

                var employee = await _employeeService.GetEmployeeByEmailAsync(userEmail);
                if (employee == null)
                {
                    TempData["Error"] = "Employee profile not found.";
                    return RedirectToAction("Index", "Login");
                }

                var model = new EmployeeProfileEditViewModel
                {
                    EmployeeId = employee.EmployeeId,
                    Phone = employee.Phone ?? string.Empty,
                    Address = employee.Address ?? string.Empty,
                    EmergencyContactName = employee.EmergencyContactName ?? string.Empty,
                    EmergencyContactPhone = employee.EmergencyContactPhone ?? string.Empty,
                    DateOfBirth = employee.DateOfBirth ?? DateTime.Today.AddYears(-25),
                    Gender = employee.Gender ?? string.Empty,
                    Nationality = employee.Nationality ?? "Indian"
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading edit profile for user: {HttpContext.Session.GetString("UserEmail")}");
                TempData["Error"] = "Error loading profile editor. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(EmployeeProfileEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var userEmail = HttpContext.Session.GetString("UserEmail");
                if (string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToAction("Index", "Login");
                }

                var employee = await _employeeService.GetEmployeeByEmailAsync(userEmail);
                if (employee == null)
                {
                    TempData["Error"] = "Employee profile not found.";
                    return RedirectToAction("Index", "Login");
                }

                // Update employee profile
                var success = await _employeeService.UpdateEmployeeProfileAsync(employee.EmployeeId, model);
                if (success)
                {
                    TempData["Success"] = "Profile updated successfully!";
                    return RedirectToAction("Profile");
                }
                else
                {
                    TempData["Error"] = "Error updating profile. Please try again.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating profile for user: {HttpContext.Session.GetString("UserEmail")}");
                TempData["Error"] = "Error updating profile. Please try again.";
                return View(model);
            }
        }

        public async Task<IActionResult> Attendance()
        {
            // TODO: Implement attendance tracking
            ViewBag.Message = "Attendance tracking coming soon!";
            return View();
        }

        public async Task<IActionResult> Payroll()
        {
            // TODO: Implement payroll view
            ViewBag.Message = "Payroll information coming soon!";
            return View();
        }

        public async Task<IActionResult> LeaveRequests()
        {
            // TODO: Implement leave request management
            ViewBag.Message = "Leave management coming soon!";
            return View();
        }

        private DateTime? GetNextBirthday(DateTime? dateOfBirth)
        {
            if (!dateOfBirth.HasValue) return null;

            var today = DateTime.Today;
            var thisYearBirthday = new DateTime(today.Year, dateOfBirth.Value.Month, dateOfBirth.Value.Day);
            
            if (thisYearBirthday < today)
            {
                return thisYearBirthday.AddYears(1);
            }
            
            return thisYearBirthday;
        }

        private double GetYearsOfService(DateTime hireDate)
        {
            var timeSpan = DateTime.Now - hireDate;
            return Math.Round(timeSpan.TotalDays / 365.25, 1);
        }
    }
}