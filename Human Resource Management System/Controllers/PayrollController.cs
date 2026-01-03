using Microsoft.AspNetCore.Mvc;
using Human_Resource_Management_System.Models;
using Human_Resource_Management_System.Services;
using Human_Resource_Management_System.Attributes;

namespace Human_Resource_Management_System.Controllers
{
    [AdminAuthorize]
    public class PayrollController : BaseController
    {
        private readonly IPayrollService _payrollService;
        private readonly ILogger<PayrollController> _logger;

        public PayrollController(IPayrollService payrollService, ILogger<PayrollController> logger)
        {
            _payrollService = payrollService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? year, int? month, string? department, string? status)
        {
            try
            {
                var selectedYear = year ?? DateTime.Today.Year;
                var selectedMonth = month ?? DateTime.Today.Month;

                List<PayrollViewModel> payrolls;
                
                if (year.HasValue && month.HasValue)
                {
                    payrolls = await _payrollService.GetPayrollByPeriodAsync(selectedYear, selectedMonth);
                }
                else
                {
                    payrolls = await _payrollService.GetAllPayrollAsync();
                }

                // Apply filters
                if (!string.IsNullOrEmpty(department) && department != "All")
                {
                    payrolls = payrolls.Where(p => p.Department == department).ToList();
                }
                if (!string.IsNullOrEmpty(status) && status != "All")
                {
                    payrolls = payrolls.Where(p => p.Status == status).ToList();
                }

                var summary = await _payrollService.GetPayrollSummaryAsync(selectedYear, selectedMonth);
                var employees = await _payrollService.GetAllActiveEmployeesAsync();

                ViewBag.SelectedYear = selectedYear;
                ViewBag.SelectedMonth = selectedMonth;
                ViewBag.Summary = summary;
                ViewBag.Employees = employees;
                ViewBag.DepartmentFilter = department ?? "";
                ViewBag.StatusFilter = status ?? "";

                return View(payrolls);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payroll list");
                TempData["Error"] = "Error loading payroll. Please try again.";
                return View(new List<PayrollViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePayrollRequest model)
        {
            try
            {
                if (model.EmployeeId <= 0)
                {
                    return Json(new { success = false, message = "Please select an employee." });
                }

                var createModel = new CreatePayrollViewModel
                {
                    EmployeeId = model.EmployeeId,
                    PayPeriodStart = new DateTime(model.Year, model.Month, 1),
                    PayPeriodEnd = new DateTime(model.Year, model.Month, 1).AddMonths(1).AddDays(-1),
                    BasicSalary = model.BasicSalary,
                    Allowances = model.Allowances,
                    Deductions = model.Deductions,
                    PayDate = model.PayDate
                };

                var success = await _payrollService.CreatePayrollAsync(createModel);

                if (success)
                {
                    return Json(new { success = true, message = "Payroll created successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to create payroll. It may already exist for this period." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating payroll for employee {model.EmployeeId}");
                return Json(new { success = false, message = "An error occurred while creating payroll." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPayroll(int id)
        {
            try
            {
                var payroll = await _payrollService.GetPayrollByIdAsync(id);
                if (payroll == null)
                {
                    return Json(new { success = false, message = "Payroll record not found." });
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        payrollId = payroll.PayrollId,
                        employeeId = payroll.EmployeeId,
                        employeeName = payroll.EmployeeName,
                        employeeCode = payroll.EmployeeCode,
                        department = payroll.Department,
                        payPeriodStart = payroll.PayPeriodStart.ToString("yyyy-MM-dd"),
                        payPeriodEnd = payroll.PayPeriodEnd.ToString("yyyy-MM-dd"),
                        basicSalary = payroll.BasicSalary,
                        allowances = payroll.Allowances,
                        deductions = payroll.Deductions,
                        netSalary = payroll.NetSalary,
                        payDate = payroll.PayDate.ToString("yyyy-MM-dd"),
                        status = payroll.Status
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting payroll: {id}");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UpdatePayrollRequest model)
        {
            try
            {
                var updateModel = new UpdatePayrollViewModel
                {
                    BasicSalary = model.BasicSalary,
                    Allowances = model.Allowances,
                    Deductions = model.Deductions,
                    PayDate = model.PayDate,
                    Status = model.Status
                };

                var success = await _payrollService.UpdatePayrollAsync(model.PayrollId, updateModel);

                if (success)
                {
                    return Json(new { success = true, message = "Payroll updated successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update payroll." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating payroll: {model.PayrollId}");
                return Json(new { success = false, message = "An error occurred while updating payroll." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _payrollService.DeletePayrollAsync(id);
                if (success)
                {
                    return Json(new { success = true, message = "Payroll record deleted successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to delete payroll record." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting payroll: {id}");
                return Json(new { success = false, message = "An error occurred while deleting payroll." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            try
            {
                var success = await _payrollService.MarkAsPaidAsync(id);
                if (success)
                {
                    return Json(new { success = true, message = "Payroll marked as paid successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to mark payroll as paid." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking payroll as paid: {id}");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayroll([FromBody] ProcessPayrollRequest model)
        {
            try
            {
                var processModel = new ProcessPayrollViewModel
                {
                    Year = model.Year,
                    Month = model.Month,
                    PayDate = model.PayDate,
                    Department = model.Department,
                    Notes = model.Notes
                };

                var success = await _payrollService.ProcessPayrollAsync(processModel);

                if (success)
                {
                    return Json(new { success = true, message = "Payroll processed successfully for all eligible employees." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to process payroll." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing payroll for {model.Year}-{model.Month}");
                return Json(new { success = false, message = "An error occurred while processing payroll." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewPayslip(int id)
        {
            try
            {
                var payroll = await _payrollService.GetPayrollByIdAsync(id);
                if (payroll == null)
                {
                    return Json(new { success = false, message = "Payroll record not found." });
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        employeeName = payroll.EmployeeName,
                        employeeCode = payroll.EmployeeCode,
                        department = payroll.Department,
                        position = payroll.Position,
                        payPeriod = payroll.PayPeriod,
                        payPeriodStart = payroll.PayPeriodStart.ToString("MMM dd, yyyy"),
                        payPeriodEnd = payroll.PayPeriodEnd.ToString("MMM dd, yyyy"),
                        payDate = payroll.PayDate.ToString("MMM dd, yyyy"),
                        basicSalary = payroll.BasicSalary,
                        allowances = payroll.Allowances,
                        grossSalary = payroll.GrossSalary,
                        deductions = payroll.Deductions,
                        netSalary = payroll.NetSalary,
                        status = payroll.Status
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error viewing payslip: {id}");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadPayslip(int id)
        {
            try
            {
                var payroll = await _payrollService.GetPayrollByIdAsync(id);
                if (payroll == null)
                {
                    TempData["Error"] = "Payroll record not found.";
                    return RedirectToAction("Index");
                }

                var payslipBytes = await _payrollService.GeneratePayslipAsync(id);
                if (payslipBytes.Length == 0)
                {
                    TempData["Error"] = "Failed to generate payslip.";
                    return RedirectToAction("Index");
                }

                var fileName = $"Payslip_{payroll.EmployeeCode}_{payroll.PayPeriodStart:yyyyMM}.txt";
                return File(payslipBytes, "text/plain", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading payslip: {id}");
                TempData["Error"] = "Error downloading payslip. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportReport(int? year, int? month)
        {
            try
            {
                var selectedYear = year ?? DateTime.Today.Year;
                var selectedMonth = month ?? DateTime.Today.Month;

                var reportBytes = await _payrollService.ExportPayrollReportAsync(selectedYear, selectedMonth);
                if (reportBytes.Length == 0)
                {
                    TempData["Error"] = "No payroll data found for the selected period.";
                    return RedirectToAction("Index");
                }

                var fileName = $"Payroll_Report_{selectedYear}_{selectedMonth:D2}.csv";
                return File(reportBytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting payroll report");
                TempData["Error"] = "Error exporting report. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EmployeePayrollHistory(int id)
        {
            try
            {
                var payrolls = await _payrollService.GetPayrollByEmployeeAsync(id);
                return Json(new { success = true, data = payrolls });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting payroll history for employee: {id}");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSummary(int year, int month)
        {
            try
            {
                var summary = await _payrollService.GetPayrollSummaryAsync(year, month);
                return Json(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting summary for {year}-{month}");
                return Json(new { success = false, message = "An error occurred." });
            }
        }
    }

    // Request models for API endpoints
    public class CreatePayrollRequest
    {
        public int EmployeeId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal Allowances { get; set; }
        public decimal Deductions { get; set; }
        public DateTime PayDate { get; set; }
    }

    public class UpdatePayrollRequest
    {
        public int PayrollId { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal Allowances { get; set; }
        public decimal Deductions { get; set; }
        public DateTime PayDate { get; set; }
        public string Status { get; set; } = "Pending";
    }

    public class ProcessPayrollRequest
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime PayDate { get; set; }
        public string? Department { get; set; }
        public string? Notes { get; set; }
    }
}