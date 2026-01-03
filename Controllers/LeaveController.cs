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
    public class LeaveController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILeaveService _leaveService;
        private readonly IWebHostEnvironment _environment;

        public LeaveController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ILeaveService leaveService,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _leaveService = leaveService;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser!);
            var isAdmin = roles.Contains("Admin");

            IQueryable<LeaveRequest> leaveRequestsQuery = _context.LeaveRequests
                .Include(l => l.Employee);

            Employee? currentEmployee = null;
            if (!isAdmin && currentUser?.EmployeeId != null)
            {
                leaveRequestsQuery = leaveRequestsQuery
                    .Where(l => l.EmployeeId == currentUser.EmployeeId.Value);
                    
                currentEmployee = await _leaveService.GetEmployeeLeaveBalanceAsync(currentUser.EmployeeId.Value);
            }

            var leaveRequests = await leaveRequestsQuery
                .OrderByDescending(l => l.RequestDate)
                .ToListAsync();

            var viewModel = new LeaveRequestViewModel
            {
                LeaveRequests = leaveRequests,
                IsAdmin = isAdmin,
                CurrentEmployee = currentEmployee
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.EmployeeId == null)
            {
                return BadRequest("Employee not found");
            }

            var employee = await _leaveService.GetEmployeeLeaveBalanceAsync(currentUser.EmployeeId.Value);
            
            var viewModel = new CreateLeaveRequestViewModel
            {
                Employee = employee
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateLeaveRequestViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.EmployeeId == null)
            {
                return BadRequest("Employee not found");
            }

            // Reload employee data for validation
            model.Employee = await _leaveService.GetEmployeeLeaveBalanceAsync(currentUser.EmployeeId.Value);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.ToDate < model.FromDate)
            {
                ModelState.AddModelError(string.Empty, "To Date must be greater than or equal to From Date");
                return View(model);
            }

            var totalDays = (model.ToDate - model.FromDate).Days + 1;

            // Check leave balance for paid and sick leave
            if (model.LeaveType != "UnpaidLeave")
            {
                var hasSufficientBalance = await _leaveService.HasSufficientLeaveBalanceAsync(
                    currentUser.EmployeeId.Value, model.LeaveType, totalDays);

                if (!hasSufficientBalance)
                {
                    var leaveTypeName = model.LeaveType == "PaidLeave" ? "Paid Time Off" : "Sick Leave";
                    var availableDays = model.LeaveType == "PaidLeave" ? 
                        model.Employee!.AvailablePTO : model.Employee!.AvailableSickDays;
                    
                    ModelState.AddModelError(string.Empty, 
                        $"Insufficient {leaveTypeName} balance. You have {availableDays} days available, but requested {totalDays} days.");
                    return View(model);
                }
            }

            string? attachmentPath = null;
            if (model.Attachment != null)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "leave-attachments");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{model.Attachment.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Attachment.CopyToAsync(fileStream);
                }

                attachmentPath = $"/uploads/leave-attachments/{uniqueFileName}";
            }

            var leaveRequest = new LeaveRequest
            {
                EmployeeId = currentUser.EmployeeId.Value,
                FromDate = model.FromDate,
                ToDate = model.ToDate,
                LeaveType = model.LeaveType,
                Reason = model.Reason,
                AttachmentPath = attachmentPath,
                Status = "Pending",
                RequestDate = DateTime.Now,
                TotalDays = totalDays
            };

            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Leave request submitted successfully! It's pending approval.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string? remarks)
        {
            var leaveRequest = await _context.LeaveRequests
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.LeaveRequestId == id);

            if (leaveRequest == null)
            {
                return NotFound();
            }

            if (leaveRequest.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Only pending leave requests can be approved.";
                return RedirectToAction(nameof(Index));
            }

            // Check leave balance one more time before approval
            if (leaveRequest.LeaveType != "UnpaidLeave")
            {
                var hasSufficientBalance = await _leaveService.HasSufficientLeaveBalanceAsync(
                    leaveRequest.EmployeeId, leaveRequest.LeaveType, leaveRequest.TotalDays);

                if (!hasSufficientBalance)
                {
                    var leaveTypeName = leaveRequest.LeaveType == "PaidLeave" ? "Paid Time Off" : "Sick Leave";
                    TempData["ErrorMessage"] = $"Cannot approve: Employee has insufficient {leaveTypeName} balance.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var currentUser = await _userManager.GetUserAsync(User);

            leaveRequest.Status = "Approved";
            leaveRequest.ApprovedDate = DateTime.Now;
            leaveRequest.ApprovedBy = currentUser?.UserName;
            leaveRequest.AdminRemarks = remarks;

            _context.LeaveRequests.Update(leaveRequest);

            // Deduct leave balance
            await _leaveService.DeductLeaveBalanceAsync(
                leaveRequest.EmployeeId, leaveRequest.LeaveType, leaveRequest.TotalDays);

            // Mark attendance as on leave
            for (var date = leaveRequest.FromDate; date <= leaveRequest.ToDate; date = date.AddDays(1))
            {
                var existingAttendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.EmployeeId == leaveRequest.EmployeeId && a.Date.Date == date.Date);

                if (existingAttendance == null)
                {
                    var attendance = new Attendance
                    {
                        EmployeeId = leaveRequest.EmployeeId,
                        Date = date,
                        Status = "OnLeave",
                        Remarks = $"Leave: {leaveRequest.LeaveType}",
                        CreatedDate = DateTime.Now
                    };
                    _context.Attendances.Add(attendance);
                }
                else
                {
                    existingAttendance.Status = "OnLeave";
                    existingAttendance.Remarks = $"Leave: {leaveRequest.LeaveType}";
                }
            }

            await _context.SaveChangesAsync();

            try
            {
                var leaveTypeName = leaveRequest.LeaveType switch
                {
                    "PaidLeave" => "Paid Leave",
                    "SickLeave" => "Sick Leave", 
                    "UnpaidLeave" => "Unpaid Leave",
                    _ => leaveRequest.LeaveType
                };

                var emailBody = $@"
                    <h2>Leave Request Approved</h2>
                    <p>Dear {leaveRequest.Employee.FirstName},</p>
                    <p>Your leave request has been <strong>approved</strong>.</p>
                    <p><strong>Details:</strong></p>
                    <ul>
                        <li>Leave Type: {leaveTypeName}</li>
                        <li>From: {leaveRequest.FromDate:dd/MM/yyyy}</li>
                        <li>To: {leaveRequest.ToDate:dd/MM/yyyy}</li>
                        <li>Total Days: {leaveRequest.TotalDays}</li>
                        {(string.IsNullOrEmpty(remarks) ? "" : $"<li>Remarks: {remarks}</li>")}
                    </ul>
                    {(leaveRequest.LeaveType != "UnpaidLeave" ? $"<p><strong>Updated Leave Balance:</strong></p><ul><li>Available PTO: {leaveRequest.Employee.AvailablePTO} days</li><li>Available Sick Days: {leaveRequest.Employee.AvailableSickDays} days</li></ul>" : "")}
                    <br/>
                    <p>Best Regards,<br/>HR Team</p>
                ";

                await _emailService.SendEmailAsync(
                    leaveRequest.Employee.Email, 
                    "Leave Request Approved", 
                    emailBody);
            }
            catch (Exception ex)
            {
                TempData["EmailWarning"] = $"Leave approved but email failed: {ex.Message}";
            }

            TempData["SuccessMessage"] = $"Leave request approved! {leaveRequest.TotalDays} days deducted from employee's balance.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? remarks)
        {
            var leaveRequest = await _context.LeaveRequests
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.LeaveRequestId == id);

            if (leaveRequest == null)
            {
                return NotFound();
            }

            if (leaveRequest.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Only pending leave requests can be rejected.";
                return RedirectToAction(nameof(Index));
            }

            var currentUser = await _userManager.GetUserAsync(User);

            leaveRequest.Status = "Rejected";
            leaveRequest.ApprovedDate = DateTime.Now;
            leaveRequest.ApprovedBy = currentUser?.UserName;
            leaveRequest.AdminRemarks = remarks;

            _context.LeaveRequests.Update(leaveRequest);
            await _context.SaveChangesAsync();

            try
            {
                var leaveTypeName = leaveRequest.LeaveType switch
                {
                    "PaidLeave" => "Paid Leave",
                    "SickLeave" => "Sick Leave", 
                    "UnpaidLeave" => "Unpaid Leave",
                    _ => leaveRequest.LeaveType
                };

                var emailBody = $@"
                    <h2>Leave Request Rejected</h2>
                    <p>Dear {leaveRequest.Employee.FirstName},</p>
                    <p>We regret to inform you that your leave request has been <strong>rejected</strong>.</p>
                    <p><strong>Details:</strong></p>
                    <ul>
                        <li>Leave Type: {leaveTypeName}</li>
                        <li>From: {leaveRequest.FromDate:dd/MM/yyyy}</li>
                        <li>To: {leaveRequest.ToDate:dd/MM/yyyy}</li>
                        {(string.IsNullOrEmpty(remarks) ? "" : $"<li>Reason: {remarks}</li>")}
                    </ul>
                    <p>Please contact HR for more information.</p>
                    <br/>
                    <p>Best Regards,<br/>HR Team</p>
                ";

                await _emailService.SendEmailAsync(
                    leaveRequest.Employee.Email, 
                    "Leave Request Rejected", 
                    emailBody);
            }
            catch (Exception ex)
            {
                TempData["EmailWarning"] = $"Leave rejected but email failed: {ex.Message}";
            }

            TempData["SuccessMessage"] = "Leave request rejected.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var leaveRequest = await _context.LeaveRequests
                .FirstOrDefaultAsync(l => l.LeaveRequestId == id);

            if (leaveRequest == null)
            {
                return NotFound();
            }

            if (leaveRequest.EmployeeId != currentUser?.EmployeeId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (leaveRequest.Status == "Approved")
            {
                // Restore leave balance if it was an approved request
                await _leaveService.RestoreLeaveBalanceAsync(
                    leaveRequest.EmployeeId, leaveRequest.LeaveType, leaveRequest.TotalDays);

                // Remove attendance records
                var attendanceRecords = await _context.Attendances
                    .Where(a => a.EmployeeId == leaveRequest.EmployeeId 
                        && a.Date.Date >= leaveRequest.FromDate.Date 
                        && a.Date.Date <= leaveRequest.ToDate.Date
                        && a.Status == "OnLeave")
                    .ToListAsync();

                _context.Attendances.RemoveRange(attendanceRecords);
            }
            else if (leaveRequest.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Only pending or approved leave requests can be cancelled.";
                return RedirectToAction(nameof(Index));
            }

            _context.LeaveRequests.Remove(leaveRequest);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = leaveRequest.Status == "Approved" ? 
                "Leave request cancelled and balance restored successfully!" : 
                "Leave request cancelled successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
