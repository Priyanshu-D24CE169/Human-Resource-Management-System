using Microsoft.AspNetCore.Mvc;
using Human_Resource_Management_System.Models;
using Human_Resource_Management_System.Services;
using System.ComponentModel.DataAnnotations;

namespace Human_Resource_Management_System.Controllers
{
    public class EmployeeController : BaseController
    {
        private readonly IEmployeeService _employeeService;
        private readonly IEmailService _emailService;
        private readonly IValidationService _validationService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(
            IEmployeeService employeeService, 
            IEmailService emailService,
            IValidationService validationService,
            IConfiguration configuration,
            ILogger<EmployeeController> logger)
        {
            _employeeService = employeeService;
            _emailService = emailService;
            _validationService = validationService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var employees = await _employeeService.GetAllEmployeesAsync();
                var employeeViewModels = employees.Select(e => new EmployeeViewModel
                {
                    EmployeeId = e.EmployeeId,
                    EmployeeCode = e.EmployeeCode,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Email = e.Email,
                    Phone = e.Phone,
                    Department = e.Department,
                    Position = e.Position,
                    HireDate = e.HireDate,
                    Salary = e.Salary,
                    Status = e.Status
                }).ToList();

                return View(employeeViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading employee list");
                TempData["Error"] = "Error loading employees. Please try again.";
                return View(new List<EmployeeViewModel>());
            }
        }

        public IActionResult Create()
        {
            var model = new EmployeeViewModel
            {
                HireDate = DateTime.Today,
                Status = "Active"
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EmployeeViewModel model)
        {
            try
            {
                _logger.LogInformation($"?? Starting employee creation for: {model.FirstName} {model.LastName}");
                _logger.LogInformation($"?? Email: {model.Email}");
                _logger.LogInformation($"?? Department: {model.Department}, Position: {model.Position}");

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("? Model validation failed");
                    foreach (var error in ModelState)
                    {
                        _logger.LogWarning($"   Field: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                    return View(model);
                }

                // Additional business validation
                _logger.LogInformation("?? Running business validation...");
                var validationResult = _validationService.ValidateEmployeeCreation(model);
                if (validationResult != ValidationResult.Success)
                {
                    _logger.LogWarning($"? Business validation failed: {validationResult.ErrorMessage}");
                    ModelState.AddModelError("", validationResult.ErrorMessage!);
                    return View(model);
                }

                // Check if employee email already exists
                _logger.LogInformation("?? Checking for duplicate email...");
                var existingEmployee = await _employeeService.GetEmployeeByEmailAsync(model.Email);
                if (existingEmployee != null)
                {
                    _logger.LogWarning($"? Duplicate email found: {model.Email}");
                    ModelState.AddModelError("Email", "An employee with this email already exists.");
                    return View(model);
                }

                // Create employee
                _logger.LogInformation("?? Creating employee in database...");
                var employee = await _employeeService.CreateEmployeeAsync(model);
                
                if (employee == null)
                {
                    _logger.LogError("?? Employee creation returned null");
                    TempData["Error"] = "Failed to create employee. Please try again.";
                    return View(model);
                }

                _logger.LogInformation($"? Employee created successfully: {employee.EmployeeCode} - {employee.FullName}");

                // Generate registration link
                var baseUrl = _configuration["ApplicationSettings:BaseUrl"] ?? "http://localhost:5194";
                var registrationLink = $"{baseUrl}/Employee/Register?token={employee.RegistrationToken}";
                
                _logger.LogInformation($"?? Registration link generated: {registrationLink}");

                // Send welcome email
                _logger.LogInformation($"?? Attempting to send welcome email to {employee.Email}...");
                var emailSent = await _emailService.SendEmployeeWelcomeEmailAsync(
                    employee.Email,
                    employee.FullName,
                    employee.EmployeeCode,
                    registrationLink,
                    "temp123" // Temporary password placeholder
                );

                if (emailSent)
                {
                    TempData["Success"] = $"Employee {employee.EmployeeCode} created successfully! Welcome email sent to {employee.Email}.";
                    _logger.LogInformation($"? Employee created and welcome email sent: {employee.EmployeeCode} - {employee.FullName}");
                }
                else
                {
                    TempData["Warning"] = $"Employee {employee.EmployeeCode} created successfully, but welcome email failed to send to {employee.Email}. You can resend the email later using the 'Resend Registration' option.";
                    _logger.LogWarning($"?? Employee created but email failed: {employee.EmployeeCode} - {employee.FullName}");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"?? Exception during employee creation for: {model.FirstName} {model.LastName}");
                _logger.LogError($"   Exception Type: {ex.GetType().Name}");
                _logger.LogError($"   Exception Message: {ex.Message}");
                _logger.LogError($"   Stack Trace: {ex.StackTrace}");
                
                TempData["Error"] = $"Error creating employee: {ex.Message}. Please check the application logs and try again.";
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                if (employee == null)
                {
                    TempData["Error"] = "Employee not found.";
                    return RedirectToAction("Index");
                }

                var model = new EmployeeViewModel
                {
                    EmployeeId = employee.EmployeeId,
                    EmployeeCode = employee.EmployeeCode,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email,
                    Phone = employee.Phone,
                    Department = employee.Department,
                    Position = employee.Position,
                    HireDate = employee.HireDate,
                    Salary = employee.Salary,
                    Status = employee.Status
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading employee for edit: {id}");
                TempData["Error"] = "Error loading employee. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EmployeeViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Check if email is being changed and if it conflicts with another employee
                var existingEmployee = await _employeeService.GetEmployeeByEmailAsync(model.Email);
                if (existingEmployee != null && existingEmployee.EmployeeId != model.EmployeeId)
                {
                    ModelState.AddModelError("Email", "An employee with this email already exists.");
                    return View(model);
                }

                var success = await _employeeService.UpdateEmployeeAsync(model.EmployeeId, model);
                if (success)
                {
                    TempData["Success"] = $"Employee {model.EmployeeCode} updated successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Error updating employee. Please try again.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating employee: {model.EmployeeId}");
                TempData["Error"] = "Error updating employee. Please try again.";
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                if (employee == null)
                {
                    return Json(new { success = false, message = "Employee not found." });
                }

                var success = await _employeeService.DeleteEmployeeAsync(id);
                if (success)
                {
                    _logger.LogInformation($"Employee deactivated: {employee.EmployeeCode} - {employee.FullName}");
                    return Json(new { success = true, message = $"Employee {employee.EmployeeCode} deactivated successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Error deactivating employee." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting employee: {id}");
                return Json(new { success = false, message = "Error deactivating employee. Please try again." });
            }
        }

        // Employee Registration Actions
        public async Task<IActionResult> Register(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Invalid registration link.";
                return RedirectToAction("Index", "Login");
            }

            try
            {
                var employee = await _employeeService.GetEmployeeByTokenAsync(token);
                if (employee == null)
                {
                    TempData["Error"] = "Invalid or expired registration link. Please contact HR for assistance.";
                    return RedirectToAction("Index", "Login");
                }

                var model = new EmployeeRegistrationViewModel
                {
                    Token = token,
                    EmployeeCode = employee.EmployeeCode,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email,
                    Department = employee.Department,
                    Position = employee.Position
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading registration page for token: {token}");
                TempData["Error"] = "Error loading registration page. Please try again.";
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register(EmployeeRegistrationViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Additional business validation
                var validationResult = _validationService.ValidateEmployeeRegistration(model);
                if (validationResult != ValidationResult.Success)
                {
                    ModelState.AddModelError("", validationResult.ErrorMessage!);
                    return View(model);
                }

                var success = await _employeeService.CompleteEmployeeRegistrationAsync(model);
                if (success)
                {
                    TempData["Success"] = "Registration completed successfully! You can now login with your email and password. Your profile is 100% complete.";
                    _logger.LogInformation($"Employee registration completed with full validation: {model.EmployeeCode}");
                    return RedirectToAction("Index", "Login");
                }
                else
                {
                    TempData["Error"] = "Registration failed. Please check your information and try again.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing registration for token: {model.Token}");
                TempData["Error"] = "Error completing registration. Please try again.";
                return View(model);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                if (employee == null)
                {
                    TempData["Error"] = "Employee not found.";
                    return RedirectToAction("Index");
                }

                return View(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading employee details: {id}");
                TempData["Error"] = "Error loading employee details. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResendRegistrationEmail(int id)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                if (employee == null)
                {
                    return Json(new { success = false, message = "Employee not found." });
                }

                if (employee.IsRegistrationComplete)
                {
                    return Json(new { success = false, message = "Employee has already completed registration." });
                }

                // Generate new registration token if expired
                string token = employee.RegistrationToken ?? "";
                if (string.IsNullOrEmpty(token) || employee.RegistrationTokenExpiry < DateTime.Now)
                {
                    token = await _employeeService.GenerateRegistrationTokenAsync(id);
                }

                // Generate registration link
                var baseUrl = _configuration["ApplicationSettings:BaseUrl"] ?? "http://localhost:5194";
                var registrationLink = $"{baseUrl}/Employee/Register?token={token}";

                // Send reminder email
                var emailSent = await _emailService.SendEmployeeRegistrationReminderAsync(
                    employee.Email,
                    employee.FullName,
                    registrationLink
                );

                if (emailSent)
                {
                    return Json(new { success = true, message = "Registration reminder sent successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to send registration reminder email." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resending registration email for employee: {id}");
                return Json(new { success = false, message = "Error sending registration reminder." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestEmail()
        {
            try
            {
                _logger.LogInformation("?? Testing email functionality...");
                
                var testResult = await _emailService.SendEmployeeWelcomeEmailAsync(
                    "test@example.com",
                    "Test User",
                    "OITESU20250001",
                    "http://localhost:5194/Employee/Register?token=test123",
                    "temp123"
                );

                if (testResult)
                {
                    return Json(new { success = true, message = "? Test email sent successfully! Check application logs for details." });
                }
                else
                {
                    return Json(new { success = false, message = "? Failed to send test email. Check application logs for errors." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "?? Error in email test");
                return Json(new { success = false, message = $"Exception during email test: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestEmailWithAddress(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return Json(new { success = false, message = "Please provide a valid email address for testing." });
                }

                _logger.LogInformation($"?? Testing email functionality to: {email}");
                
                var testResult = await _emailService.SendEmployeeWelcomeEmailAsync(
                    email,
                    "Test User",
                    "OITESU20250001",
                    "http://localhost:5194/Employee/Register?token=test123",
                    "temp123"
                );

                if (testResult)
                {
                    return Json(new { success = true, message = $"? Test email sent successfully to {email}! Check your inbox and spam folder." });
                }
                else
                {
                    return Json(new { success = false, message = "? Failed to send test email. Check application logs for detailed error information." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "?? Error in email test");
                return Json(new { success = false, message = $"Exception during email test: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult EmailTest()
        {
            return View();
        }
    }
}