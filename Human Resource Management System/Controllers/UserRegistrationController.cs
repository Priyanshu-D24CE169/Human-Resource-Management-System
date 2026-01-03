using Microsoft.AspNetCore.Mvc;
using Human_Resource_Management_System.Models;
using Human_Resource_Management_System.Services;

namespace Human_Resource_Management_System.Controllers
{
    public class UserRegistrationController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEmployeeService _employeeService;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserRegistrationController> _logger;

        public UserRegistrationController(
            IUserService userService,
            IEmployeeService employeeService,
            IEmailService emailService,
            ILogger<UserRegistrationController> logger)
        {
            _userService = userService;
            _employeeService = employeeService;
            _emailService = emailService;
            _logger = logger;
        }

        // GET: UserRegistration/Register
        [HttpGet]
        public IActionResult Register()
        {
            var model = new UserRegistrationViewModel
            {
                HireDate = DateTime.Today,
                DateOfBirth = DateTime.Today.AddYears(-25), // Default to 25 years old
                Nationality = "Indian",
                Role = "Employee",
                Status = "Active"
            };

            return View(model);
        }

        // POST: UserRegistration/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserRegistrationViewModel model)
        {
            try
            {
                _logger.LogInformation($"?? Processing registration for: {model.FirstName} {model.LastName}");

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("? Model validation failed for user registration");
                    foreach (var error in ModelState)
                    {
                        foreach (var errorMsg in error.Value.Errors)
                        {
                            _logger.LogWarning($"   Field: {error.Key}, Error: {errorMsg.ErrorMessage}");
                        }
                    }
                    return View(model);
                }

                // Additional business validation
                if (model.Age < 18)
                {
                    ModelState.AddModelError("DateOfBirth", "You must be at least 18 years old to register.");
                    return View(model);
                }

                if (model.Age > 65)
                {
                    ModelState.AddModelError("DateOfBirth", "Registration is available for individuals under 65 years of age.");
                    return View(model);
                }

                if (model.HireDate < DateTime.Today)
                {
                    ModelState.AddModelError("HireDate", "Hire date cannot be in the past.");
                    return View(model);
                }

                // Create the user with complete profile
                var result = await _userService.CreateCompleteUserAsync(model);
                
                if (!result.Success)
                {
                    _logger.LogWarning($"? Registration failed: {result.Message}");
                    ModelState.AddModelError("", result.Message);
                    return View(model);
                }

                _logger.LogInformation($"? User registration successful: {model.FirstName} {model.LastName}");

                // Redirect to confirmation page
                return RedirectToAction("Confirmation", new { userId = result.UserId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"?? Exception during registration for: {model.FirstName} {model.LastName}");
                ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                return View(model);
            }
        }

        // GET: UserRegistration/Confirmation
        [HttpGet]
        public async Task<IActionResult> Confirmation(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("?? Confirmation accessed without user ID");
                    return RedirectToAction("Register");
                }

                var confirmationData = await _userService.GetRegistrationConfirmationAsync(userId);
                if (confirmationData == null)
                {
                    _logger.LogWarning($"? Confirmation data not found for user ID: {userId}");
                    TempData["Error"] = "Registration confirmation data not found.";
                    return RedirectToAction("Register");
                }

                _logger.LogInformation($"?? Displaying registration confirmation for: {confirmationData.FirstName} {confirmationData.LastName}");

                // Send confirmation email
                await _userService.SendRegistrationConfirmationEmailAsync(confirmationData.Email, confirmationData);

                return View(confirmationData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"?? Error loading confirmation page for user ID: {userId}");
                TempData["Error"] = "An error occurred loading the confirmation page.";
                return RedirectToAction("Register");
            }
        }

        // GET: UserRegistration/Success
        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }

        // API endpoint to check email availability
        [HttpPost]
        public async Task<JsonResult> CheckEmailAvailability(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return Json(new { available = false, message = "Email is required" });
                }

                var user = await _userService.GetUserByEmailAsync(email);
                var available = user == null;

                return Json(new { 
                    available = available, 
                    message = available ? "Email is available" : "Email is already registered" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking email availability: {email}");
                return Json(new { available = false, message = "Error checking email availability" });
            }
        }

        // GET: UserRegistration/GetDepartments
        [HttpGet]
        public JsonResult GetDepartments()
        {
            var departments = new[]
            {
                "Human Resources",
                "Information Technology",
                "Finance",
                "Marketing",
                "Sales",
                "Operations",
                "Research & Development",
                "Customer Service",
                "Legal",
                "Administration"
            };

            return Json(departments);
        }

        // GET: UserRegistration/GetPositions
        [HttpGet]
        public JsonResult GetPositions(string department)
        {
            var positions = department switch
            {
                "Human Resources" => new[] { "HR Manager", "HR Executive", "Recruiter", "Training Specialist" },
                "Information Technology" => new[] { "Software Developer", "System Administrator", "DevOps Engineer", "QA Engineer", "Technical Lead" },
                "Finance" => new[] { "Finance Manager", "Accountant", "Financial Analyst", "Auditor" },
                "Marketing" => new[] { "Marketing Manager", "Digital Marketing Specialist", "Content Creator", "Brand Manager" },
                "Sales" => new[] { "Sales Manager", "Sales Executive", "Account Manager", "Business Development Manager" },
                "Operations" => new[] { "Operations Manager", "Process Analyst", "Supply Chain Manager", "Logistics Coordinator" },
                "Research & Development" => new[] { "R&D Manager", "Research Scientist", "Product Developer", "Innovation Specialist" },
                "Customer Service" => new[] { "Customer Service Manager", "Customer Support Executive", "Client Relations Manager" },
                "Legal" => new[] { "Legal Advisor", "Compliance Officer", "Contract Manager" },
                "Administration" => new[] { "Administrative Manager", "Office Administrator", "Executive Assistant" },
                _ => new[] { "Employee", "Associate", "Executive", "Manager" }
            };

            return Json(positions);
        }
    }
}