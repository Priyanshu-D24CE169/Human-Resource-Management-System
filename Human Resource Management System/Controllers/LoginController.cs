using Microsoft.AspNetCore.Mvc;
using Human_Resource_Management_System.Models;
using Human_Resource_Management_System.Services;

namespace Human_Resource_Management_System.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<LoginController> _logger;

        public LoginController(IUserService userService, ILogger<LoginController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Clear any existing session
            HttpContext.Session.Clear();
            _logger.LogInformation("?? Login page accessed, session cleared");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            try
            {
                _logger.LogInformation($"?? Login attempt for email: {model?.Email ?? "null"}");
                
                if (ModelState.IsValid)
                {
                    var user = await _userService.AuthenticateAsync(model.Email, model.Password);
                    
                    if (user != null)
                    {
                        // Store user info in session
                        HttpContext.Session.SetString("UserId", user.UserId.ToString());
                        HttpContext.Session.SetString("UserEmail", user.Email);
                        HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");
                        HttpContext.Session.SetString("UserRole", user.Role);
                        
                        _logger.LogInformation($"? Login successful for user: {user.Email}, Role: {user.Role}");
                        _logger.LogInformation($"?? Redirecting to Dashboard");
                        
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else
                    {
                        _logger.LogWarning($"? Login failed for email: {model.Email}");
                        ModelState.AddModelError("", "Invalid email or password. Please check your credentials and try again.");
                    }
                }
                else
                {
                    _logger.LogWarning("? Model validation failed");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        _logger.LogDebug($"   Validation error: {error.ErrorMessage}");
                    }
                }
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"?? Login error for email: {model?.Email ?? "unknown"}");
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
                return View(model);
            }
        }

        public IActionResult Logout()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            HttpContext.Session.Clear();
            _logger.LogInformation($"?? User logged out: {userEmail ?? "unknown"}");
            return RedirectToAction("Index");
        }
    }
}