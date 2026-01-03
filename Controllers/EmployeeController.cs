using hrms.Data;
using hrms.Models;
using hrms.Services;
using hrms.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hrms.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmployeeService _employeeService;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _environment;

        public EmployeeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmployeeService employeeService,
            IEmailService emailService,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _employeeService = employeeService;
            _emailService = emailService;
            _environment = environment;
        }

        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .Include(e => e.Salary)
                .Where(e => e.IsActive)
                .OrderBy(e => e.FirstName)
                .ToListAsync();

            return View(employees);
        }

        public async Task<IActionResult> Profile(int? id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser!);
            var isAdmin = roles.Contains("Admin");

            int employeeId;
            if (id.HasValue && isAdmin)
            {
                employeeId = id.Value;
            }
            else if (currentUser?.EmployeeId != null)
            {
                employeeId = currentUser.EmployeeId.Value;
            }
            else
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Salary)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
            {
                return NotFound();
            }

            var viewModel = new EmployeeProfileViewModel
            {
                Employee = employee,
                Salary = employee.Salary,
                CanEditSalary = isAdmin,
                CanEdit = isAdmin || currentUser?.EmployeeId == employeeId
            };

            return View(viewModel);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEmployeeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var loginId = await _employeeService.GenerateLoginIdAsync(
                    model.FirstName, 
                    model.LastName, 
                    model.JoiningDate);

                var password = _employeeService.GenerateRandomPassword();

                string? profilePhotoPath = null;
                if (model.ProfilePhoto != null)
                {
                    profilePhotoPath = await SaveFileAsync(model.ProfilePhoto, "profiles");
                }

                string? resumePath = null;
                if (model.Resume != null)
                {
                    resumePath = await SaveFileAsync(model.Resume, "resumes");
                }

                var serialNumber = await _employeeService.GetNextSerialNumberAsync(model.JoiningDate.Year);
                var companySettings = await _context.CompanySettings.FirstOrDefaultAsync();

                var employee = new Employee
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    LoginId = loginId,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    City = model.City,
                    State = model.State,
                    ZipCode = model.ZipCode,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    Department = model.Department,
                    Designation = model.Designation,
                    JoiningDate = model.JoiningDate,
                    CompanyCode = companySettings?.CompanyCode ?? "OI",
                    YearOfJoining = model.JoiningDate.Year,
                    SerialNumber = serialNumber,
                    ProfilePhotoPath = profilePhotoPath,
                    ResumePath = resumePath,
                    EmergencyContactName = model.EmergencyContactName,
                    EmergencyContactPhone = model.EmergencyContactPhone,
                    BankName = model.BankName,
                    AccountNumber = model.AccountNumber,
                    IFSCCode = model.IFSCCode,
                    PANNumber = model.PANNumber,
                    AadharNumber = model.AadharNumber,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                var user = new ApplicationUser
                {
                    UserName = loginId,
                    Email = model.Email,
                    LoginId = loginId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    MustChangePassword = true,
                    EmployeeId = employee.EmployeeId,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Employee");

                    employee.UserId = user.Id;
                    _context.Employees.Update(employee);

                    var salary = new Salary
                    {
                        EmployeeId = employee.EmployeeId,
                        MonthlyWage = model.MonthlySalary,
                        YearlyWage = model.MonthlySalary * 12,
                        WorkingDaysPerMonth = 26,
                        PerDayWage = model.MonthlySalary / 26,
                        Basic = model.MonthlySalary * 0.50m,
                        HRA = model.MonthlySalary * 0.20m,
                        PF = model.MonthlySalary * 0.12m,
                        Allowances = model.MonthlySalary * 0.10m,
                        Deductions = 0,
                        ProfessionalTax = 200,
                        EffectiveFrom = model.JoiningDate,
                        CreatedDate = DateTime.Now
                    };

                    _context.Salaries.Add(salary);
                    await _context.SaveChangesAsync();

                    // Send email with credentials
                    try
                    {
                        var emailBody = $@"
                            <html>
                            <body style='font-family: Arial, sans-serif;'>
                                <h2 style='color: #0066cc;'>Welcome to HRMS System!</h2>
                                <p>Dear {model.FirstName} {model.LastName},</p>
                                <p>Your employee account has been created successfully.</p>
                                
                                <div style='background-color: #f5f5f5; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                                    <h3 style='margin-top: 0;'>Login Credentials:</h3>
                                    <p><strong>Login ID:</strong> {loginId}</p>
                                    <p><strong>Temporary Password:</strong> {password}</p>
                                </div>
                                
                                <p style='color: #d9534f;'><strong>Important:</strong> Please login and change your password immediately for security reasons.</p>
                                <p>Login URL: <a href='https://localhost:5001' style='color: #0066cc;'>HRMS Portal</a></p>
                                
                                <br/>
                                <p>Best Regards,<br/><strong>HR Team</strong></p>
                                <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'/>
                                <p style='font-size: 12px; color: #999;'>This is an automated email. Please do not reply.</p>
                            </body>
                            </html>
                        ";

                        await _emailService.SendEmailAsync(model.Email, "HRMS - Your Login Credentials", emailBody);
                        
                        TempData["SuccessMessage"] = $"Employee created successfully! Login ID: {loginId}. Credentials have been sent to {model.Email}";
                    }
                    catch (Exception ex)
                    {
                        // Log the detailed error
                        var logger = HttpContext.RequestServices.GetRequiredService<ILogger<EmployeeController>>();
                        logger.LogError($"Failed to send email to {model.Email}: {ex.Message}");
                        
                        TempData["EmailWarning"] = $"Employee created successfully (Login ID: {loginId}), but email could not be sent to {model.Email}. Error: {ex.Message}. Please provide credentials manually: Login ID: {loginId}, Password: {password}";
                    }

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating employee: {ex.Message}");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser!);
            var isAdmin = roles.Contains("Admin");

            if (!isAdmin && currentUser?.EmployeeId != id)
            {
                return Forbid();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee, IFormFile? profilePhoto)
        {
            if (id != employee.EmployeeId)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser!);
            var isAdmin = roles.Contains("Admin");

            if (!isAdmin && currentUser?.EmployeeId != id)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingEmployee = await _context.Employees.AsNoTracking()
                        .FirstOrDefaultAsync(e => e.EmployeeId == id);

                    if (profilePhoto != null)
                    {
                        employee.ProfilePhotoPath = await SaveFileAsync(profilePhoto, "profiles");
                    }
                    else
                    {
                        employee.ProfilePhotoPath = existingEmployee?.ProfilePhotoPath;
                    }

                    employee.LoginId = existingEmployee!.LoginId;
                    employee.CompanyCode = existingEmployee.CompanyCode;
                    employee.YearOfJoining = existingEmployee.YearOfJoining;
                    employee.SerialNumber = existingEmployee.SerialNumber;
                    employee.UserId = existingEmployee.UserId;
                    employee.CreatedDate = existingEmployee.CreatedDate;

                    _context.Update(employee);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction(nameof(Profile), new { id = employee.EmployeeId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.EmployeeId))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            return View(employee);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSalary(int employeeId, Salary salary)
        {
            if (employeeId != salary.EmployeeId)
            {
                return NotFound();
            }

            var existingSalary = await _context.Salaries
                .FirstOrDefaultAsync(s => s.EmployeeId == employeeId);

            if (existingSalary == null)
            {
                salary.CreatedDate = DateTime.Now;
                salary.EffectiveFrom = DateTime.Now;
                _context.Salaries.Add(salary);
            }
            else
            {
                salary.SalaryId = existingSalary.SalaryId;
                salary.CreatedDate = existingSalary.CreatedDate;
                salary.UpdatedDate = DateTime.Now;
                _context.Entry(existingSalary).CurrentValues.SetValues(salary);
            }

            salary.YearlyWage = salary.MonthlyWage * 12;
            salary.PerDayWage = salary.MonthlyWage / salary.WorkingDaysPerMonth;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Salary updated successfully!";
            return RedirectToAction(nameof(Profile), new { id = employeeId });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordAndResend(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Employee not found.";
                return RedirectToAction(nameof(Index));
            }

            // Check if UserId exists
            if (string.IsNullOrEmpty(employee.UserId))
            {
                TempData["ErrorMessage"] = "User account not found for this employee. Please recreate the employee account.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(employee.UserId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User account not found for this employee. The account may have been deleted.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Generate new random password
                var newPassword = _employeeService.GenerateRandomPassword();

                // Reset password token
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    TempData["ErrorMessage"] = $"Failed to reset password: {errors}";
                    return RedirectToAction(nameof(Index));
                }

                // Set flag to force password change
                user.MustChangePassword = true;
                await _userManager.UpdateAsync(user);

                // Send email with new credentials
                try
                {
                    var emailBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2 style='color: #0066cc;'>HRMS - Password Reset</h2>
                            <p>Dear {employee.FirstName} {employee.LastName},</p>
                            <p>Your password has been reset by the administrator.</p>
                            
                            <div style='background-color: #f5f5f5; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                                <h3 style='margin-top: 0;'>New Login Credentials:</h3>
                                <p><strong>Login ID:</strong> {employee.LoginId}</p>
                                <p><strong>New Temporary Password:</strong> {newPassword}</p>
                            </div>
                            
                            <p style='color: #d9534f;'><strong>Important:</strong> Please login and change your password immediately for security reasons.</p>
                            <p>Login URL: <a href='{Request.Scheme}://{Request.Host}' style='color: #0066cc;'>HRMS Portal</a></p>
                            
                            <br/>
                            <p>Best Regards,<br/><strong>HR Team</strong></p>
                            <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'/>
                            <p style='font-size: 12px; color: #999;'>This is an automated email. Please do not reply.</p>
                        </body>
                        </html>
                    ";

                    await _emailService.SendEmailAsync(employee.Email, "HRMS - Password Reset & Credentials", emailBody);
                    
                    TempData["SuccessMessage"] = $"Password reset successfully! New credentials have been sent to {employee.Email}";
                }
                catch (Exception ex)
                {
                    var logger = HttpContext.RequestServices.GetRequiredService<ILogger<EmployeeController>>();
                    logger.LogError($"Failed to send password reset email to {employee.Email}: {ex.Message}");
                    
                    TempData["EmailWarning"] = $"Password reset successfully, but email could not be sent to {employee.Email}. Please provide new credentials manually: Login ID: {employee.LoginId}, Password: {newPassword}";
                }
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<EmployeeController>>();
                logger.LogError($"Error resetting password for employee {id}: {ex.Message}");
                
                TempData["ErrorMessage"] = $"Error resetting password: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendCredentials(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Employee not found.";
                return RedirectToAction(nameof(Index));
            }

            // Check if UserId exists
            if (string.IsNullOrEmpty(employee.UserId))
            {
                TempData["ErrorMessage"] = "User account not found for this employee. Please recreate the employee account.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(employee.UserId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User account not found for this employee. The account may have been deleted.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var emailBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2 style='color: #0066cc;'>HRMS - Login Credentials Reminder</h2>
                        <p>Dear {employee.FirstName} {employee.LastName},</p>
                        <p>As requested, here are your login credentials for the HRMS system.</p>
                        
                        <div style='background-color: #f5f5f5; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                            <h3 style='margin-top: 0;'>Login Credentials:</h3>
                            <p><strong>Login ID:</strong> {employee.LoginId}</p>
                            <p style='color: #d9534f;'><em>Password: (Your current password - if forgotten, request password reset)</em></p>
                        </div>
                        
                        <p><strong>Note:</strong> If you've forgotten your password, please contact the HR administrator to request a password reset.</p>
                        <p>Login URL: <a href='{Request.Scheme}://{Request.Host}' style='color: #0066cc;'>HRMS Portal</a></p>
                        
                        <br/>
                        <p>Best Regards,<br/><strong>HR Team</strong></p>
                        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'/>
                        <p style='font-size: 12px; color: #999;'>This is an automated email. Please do not reply.</p>
                    </body>
                    </html>
                ";

                await _emailService.SendEmailAsync(employee.Email, "HRMS - Login Credentials Reminder", emailBody);
                
                TempData["SuccessMessage"] = $"Login ID has been sent to {employee.Email}. For password reset, use the 'Reset Password' option.";
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<EmployeeController>>();
                logger.LogError($"Failed to send credentials reminder to {employee.Email}: {ex.Message}");
                
                TempData["ErrorMessage"] = $"Failed to send email to {employee.Email}. Error: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecreateUserAccount(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Employee not found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Check if user account already exists by LoginId
                var existingUser = await _userManager.FindByNameAsync(employee.LoginId);
                if (existingUser != null)
                {
                    // Link existing user to employee
                    employee.UserId = existingUser.Id;
                    _context.Employees.Update(employee);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"User account linked successfully! Use 'Reset Password' to send new credentials.";
                    return RedirectToAction(nameof(Index));
                }

                // Generate new password
                var password = _employeeService.GenerateRandomPassword();

                // Create new user account
                var user = new ApplicationUser
                {
                    UserName = employee.LoginId,
                    Email = employee.Email,
                    LoginId = employee.LoginId,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    MustChangePassword = true,
                    EmployeeId = employee.EmployeeId,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    TempData["ErrorMessage"] = $"Failed to create user account: {errors}";
                    return RedirectToAction(nameof(Index));
                }

                // Assign Employee role
                await _userManager.AddToRoleAsync(user, "Employee");

                // Link user to employee
                employee.UserId = user.Id;
                _context.Employees.Update(employee);
                await _context.SaveChangesAsync();

                // Send email with credentials
                try
                {
                    var emailBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2 style='color: #0066cc;'>HRMS - User Account Created</h2>
                            <p>Dear {employee.FirstName} {employee.LastName},</p>
                            <p>Your user account has been recreated by the administrator.</p>
                            
                            <div style='background-color: #f5f5f5; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                                <h3 style='margin-top: 0;'>Login Credentials:</h3>
                                <p><strong>Login ID:</strong> {employee.LoginId}</p>
                                <p><strong>Temporary Password:</strong> {password}</p>
                            </div>
                            
                            <p style='color: #d9534f;'><strong>Important:</strong> Please login and change your password immediately for security reasons.</p>
                            <p>Login URL: <a href='{Request.Scheme}://{Request.Host}' style='color: #0066cc;'>HRMS Portal</a></p>
                            
                            <br/>
                            <p>Best Regards,<br/><strong>HR Team</strong></p>
                            <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'/>
                            <p style='font-size: 12px; color: #999;'>This is an automated email. Please do not reply.</p>
                        </body>
                        </html>
                    ";

                    await _emailService.SendEmailAsync(employee.Email, "HRMS - User Account Created", emailBody);
                    
                    TempData["SuccessMessage"] = $"User account created successfully! Credentials have been sent to {employee.Email}";
                }
                catch (Exception ex)
                {
                    var logger = HttpContext.RequestServices.GetRequiredService<ILogger<EmployeeController>>();
                    logger.LogError($"Failed to send email to {employee.Email}: {ex.Message}");
                    
                    TempData["EmailWarning"] = $"User account created successfully, but email could not be sent to {employee.Email}. Please provide credentials manually: Login ID: {employee.LoginId}, Password: {password}";
                }
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<EmployeeController>>();
                logger.LogError($"Error recreating user account for employee {id}: {ex.Message}");
                
                TempData["ErrorMessage"] = $"Error recreating user account: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeeId == id);
        }

        private async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{folder}/{uniqueFileName}";
        }
    }
}
