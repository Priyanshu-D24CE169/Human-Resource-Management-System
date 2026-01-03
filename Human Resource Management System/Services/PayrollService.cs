using Human_Resource_Management_System.Data;
using Human_Resource_Management_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Human_Resource_Management_System.Services
{
    public interface IPayrollService
    {
        Task<List<PayrollViewModel>> GetAllPayrollAsync();
        Task<List<PayrollViewModel>> GetPayrollByPeriodAsync(int year, int month);
        Task<List<PayrollViewModel>> GetPayrollByEmployeeAsync(int employeeId);
        Task<PayrollViewModel?> GetPayrollByIdAsync(int payrollId);
        Task<Payroll?> GetPayrollRecordAsync(int employeeId, int year, int month);
        Task<bool> CreatePayrollAsync(CreatePayrollViewModel model);
        Task<bool> UpdatePayrollAsync(int payrollId, UpdatePayrollViewModel model);
        Task<bool> DeletePayrollAsync(int payrollId);
        Task<bool> MarkAsPaidAsync(int payrollId);
        Task<bool> ProcessPayrollAsync(ProcessPayrollViewModel model);
        Task<PayrollSummary> GetPayrollSummaryAsync(int year, int month);
        Task<List<Employee>> GetAllActiveEmployeesAsync();
        Task<byte[]> GeneratePayslipAsync(int payrollId);
        Task<byte[]> ExportPayrollReportAsync(int year, int month);
    }

    public class PayrollService : IPayrollService
    {
        private readonly HrmsDbContext _context;
        private readonly ILogger<PayrollService> _logger;

        public PayrollService(HrmsDbContext context, ILogger<PayrollService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PayrollViewModel>> GetAllPayrollAsync()
        {
            try
            {
                var payrolls = await _context.Payrolls
                    .Include(p => p.Employee)
                    .OrderByDescending(p => p.PayPeriodEnd)
                    .ThenBy(p => p.Employee.FirstName)
                    .ToListAsync();

                return payrolls.Select(MapToViewModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all payroll records");
                return new List<PayrollViewModel>();
            }
        }

        public async Task<List<PayrollViewModel>> GetPayrollByPeriodAsync(int year, int month)
        {
            try
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var payrolls = await _context.Payrolls
                    .Include(p => p.Employee)
                    .Where(p => p.PayPeriodStart >= startDate && p.PayPeriodEnd <= endDate)
                    .OrderBy(p => p.Employee.FirstName)
                    .ToListAsync();

                return payrolls.Select(MapToViewModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting payroll for period: {year}-{month}");
                return new List<PayrollViewModel>();
            }
        }

        public async Task<List<PayrollViewModel>> GetPayrollByEmployeeAsync(int employeeId)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(employeeId);
                if (employee == null) return new List<PayrollViewModel>();

                var payrolls = await _context.Payrolls
                    .Include(p => p.Employee)
                    .Where(p => p.EmployeeId == employeeId)
                    .OrderByDescending(p => p.PayPeriodEnd)
                    .ToListAsync();

                return payrolls.Select(MapToViewModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting payroll for employee: {employeeId}");
                return new List<PayrollViewModel>();
            }
        }

        public async Task<PayrollViewModel?> GetPayrollByIdAsync(int payrollId)
        {
            try
            {
                var payroll = await _context.Payrolls
                    .Include(p => p.Employee)
                    .FirstOrDefaultAsync(p => p.PayrollId == payrollId);

                return payroll != null ? MapToViewModel(payroll) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting payroll by ID: {payrollId}");
                return null;
            }
        }

        public async Task<Payroll?> GetPayrollRecordAsync(int employeeId, int year, int month)
        {
            try
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                return await _context.Payrolls
                    .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && 
                                             p.PayPeriodStart >= startDate && 
                                             p.PayPeriodEnd <= endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting payroll record for employee {employeeId} in {year}-{month}");
                return null;
            }
        }

        public async Task<bool> CreatePayrollAsync(CreatePayrollViewModel model)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(model.EmployeeId);
                if (employee == null)
                {
                    _logger.LogWarning($"Employee not found: {model.EmployeeId}");
                    return false;
                }

                // Check if payroll already exists for this period
                var existingPayroll = await GetPayrollRecordAsync(model.EmployeeId, model.PayPeriodStart.Year, model.PayPeriodStart.Month);
                if (existingPayroll != null)
                {
                    _logger.LogWarning($"Payroll already exists for employee {model.EmployeeId} in {model.PayPeriodStart:yyyy-MM}");
                    return false;
                }

                var payroll = new Payroll
                {
                    EmployeeId = model.EmployeeId,
                    PayPeriodStart = model.PayPeriodStart,
                    PayPeriodEnd = model.PayPeriodEnd,
                    BasicSalary = model.BasicSalary,
                    Allowances = model.Allowances,
                    Deductions = model.Deductions,
                    NetSalary = model.BasicSalary + model.Allowances - model.Deductions,
                    PayDate = model.PayDate,
                    Status = "Pending",
                    CreatedDate = DateTime.Now
                };

                _context.Payrolls.Add(payroll);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Payroll created for employee {employee.FullName} - Period: {model.PayPeriodStart:yyyy-MM}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating payroll for employee {model.EmployeeId}");
                return false;
            }
        }

        public async Task<bool> UpdatePayrollAsync(int payrollId, UpdatePayrollViewModel model)
        {
            try
            {
                var payroll = await _context.Payrolls.FindAsync(payrollId);
                if (payroll == null)
                {
                    _logger.LogWarning($"Payroll not found: {payrollId}");
                    return false;
                }

                payroll.BasicSalary = model.BasicSalary;
                payroll.Allowances = model.Allowances;
                payroll.Deductions = model.Deductions;
                payroll.NetSalary = model.BasicSalary + model.Allowances - model.Deductions;
                payroll.PayDate = model.PayDate;
                payroll.Status = model.Status;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Payroll updated: {payrollId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating payroll: {payrollId}");
                return false;
            }
        }

        public async Task<bool> DeletePayrollAsync(int payrollId)
        {
            try
            {
                var payroll = await _context.Payrolls.FindAsync(payrollId);
                if (payroll == null)
                {
                    _logger.LogWarning($"Payroll not found for deletion: {payrollId}");
                    return false;
                }

                _context.Payrolls.Remove(payroll);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Payroll deleted: {payrollId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting payroll: {payrollId}");
                return false;
            }
        }

        public async Task<bool> MarkAsPaidAsync(int payrollId)
        {
            try
            {
                var payroll = await _context.Payrolls.FindAsync(payrollId);
                if (payroll == null)
                {
                    _logger.LogWarning($"Payroll not found: {payrollId}");
                    return false;
                }

                payroll.Status = "Paid";
                payroll.PayDate = DateTime.Today;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Payroll marked as paid: {payrollId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking payroll as paid: {payrollId}");
                return false;
            }
        }

        public async Task<bool> ProcessPayrollAsync(ProcessPayrollViewModel model)
        {
            try
            {
                var startDate = new DateTime(model.Year, model.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                // Get all active employees
                var employeesQuery = _context.Employees.Where(e => e.Status == "Active");
                
                if (!string.IsNullOrEmpty(model.Department) && model.Department != "All")
                {
                    employeesQuery = employeesQuery.Where(e => e.Department == model.Department);
                }

                var employees = await employeesQuery.ToListAsync();

                var processedCount = 0;
                var skippedCount = 0;

                foreach (var employee in employees)
                {
                    // Check if payroll already exists
                    var existingPayroll = await GetPayrollRecordAsync(employee.EmployeeId, model.Year, model.Month);
                    if (existingPayroll != null)
                    {
                        skippedCount++;
                        continue;
                    }

                    // Calculate payroll components based on employee salary
                    var basicSalary = employee.Salary / 12; // Monthly basic salary
                    var allowances = CalculateAllowances(basicSalary);
                    var deductions = CalculateDeductions(basicSalary);
                    var netSalary = basicSalary + allowances - deductions;

                    var payroll = new Payroll
                    {
                        EmployeeId = employee.EmployeeId,
                        PayPeriodStart = startDate,
                        PayPeriodEnd = endDate,
                        BasicSalary = basicSalary,
                        Allowances = allowances,
                        Deductions = deductions,
                        NetSalary = netSalary,
                        PayDate = model.PayDate,
                        Status = "Processing",
                        CreatedDate = DateTime.Now
                    };

                    _context.Payrolls.Add(payroll);
                    processedCount++;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Payroll processed: {processedCount} employees, {skippedCount} skipped (already exists)");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing payroll for {model.Year}-{model.Month}");
                return false;
            }
        }

        public async Task<PayrollSummary> GetPayrollSummaryAsync(int year, int month)
        {
            try
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var payrolls = await _context.Payrolls
                    .Where(p => p.PayPeriodStart >= startDate && p.PayPeriodEnd <= endDate)
                    .ToListAsync();

                return new PayrollSummary
                {
                    Year = year,
                    Month = month,
                    TotalEmployees = payrolls.Count,
                    TotalBasicSalary = payrolls.Sum(p => p.BasicSalary),
                    TotalAllowances = payrolls.Sum(p => p.Allowances),
                    TotalDeductions = payrolls.Sum(p => p.Deductions),
                    TotalNetSalary = payrolls.Sum(p => p.NetSalary),
                    PaidCount = payrolls.Count(p => p.Status == "Paid"),
                    PendingCount = payrolls.Count(p => p.Status == "Pending"),
                    ProcessingCount = payrolls.Count(p => p.Status == "Processing")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting payroll summary for {year}-{month}");
                return new PayrollSummary { Year = year, Month = month };
            }
        }

        public async Task<List<Employee>> GetAllActiveEmployeesAsync()
        {
            try
            {
                return await _context.Employees
                    .Where(e => e.Status == "Active")
                    .OrderBy(e => e.FirstName)
                    .ThenBy(e => e.LastName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active employees");
                return new List<Employee>();
            }
        }

        public async Task<byte[]> GeneratePayslipAsync(int payrollId)
        {
            try
            {
                var payroll = await _context.Payrolls
                    .Include(p => p.Employee)
                    .FirstOrDefaultAsync(p => p.PayrollId == payrollId);

                if (payroll == null) return Array.Empty<byte>();

                // Generate simple text-based payslip (in real app, use PDF library)
                var payslip = GeneratePayslipContent(payroll);
                return System.Text.Encoding.UTF8.GetBytes(payslip);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating payslip for payroll: {payrollId}");
                return Array.Empty<byte>();
            }
        }

        public async Task<byte[]> ExportPayrollReportAsync(int year, int month)
        {
            try
            {
                var payrolls = await GetPayrollByPeriodAsync(year, month);

                var csv = new System.Text.StringBuilder();
                csv.AppendLine("Employee Code,Employee Name,Department,Basic Salary,Allowances,Deductions,Net Salary,Pay Date,Status");

                foreach (var payroll in payrolls)
                {
                    csv.AppendLine($"{payroll.EmployeeCode},{payroll.EmployeeName},{payroll.Department},{payroll.BasicSalary},{payroll.Allowances},{payroll.Deductions},{payroll.NetSalary},{payroll.PayDate:yyyy-MM-dd},{payroll.Status}");
                }

                return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error exporting payroll report for {year}-{month}");
                return Array.Empty<byte>();
            }
        }

        private PayrollViewModel MapToViewModel(Payroll payroll)
        {
            return new PayrollViewModel
            {
                PayrollId = payroll.PayrollId,
                EmployeeId = payroll.EmployeeId,
                EmployeeCode = payroll.Employee?.EmployeeCode ?? "",
                EmployeeName = payroll.Employee?.FullName ?? "",
                Department = payroll.Employee?.Department ?? "",
                Position = payroll.Employee?.Position ?? "",
                PayPeriodStart = payroll.PayPeriodStart,
                PayPeriodEnd = payroll.PayPeriodEnd,
                BasicSalary = payroll.BasicSalary,
                Allowances = payroll.Allowances,
                Deductions = payroll.Deductions,
                NetSalary = payroll.NetSalary,
                PayDate = payroll.PayDate,
                Status = payroll.Status
            };
        }

        private decimal CalculateAllowances(decimal basicSalary)
        {
            // Standard allowance calculation (can be customized)
            // HRA: 40% of basic, Transport: 1600, Medical: 1250
            var hra = basicSalary * 0.40m;
            var transport = 1600m;
            var medical = 1250m;
            return Math.Round(hra + transport + medical, 2);
        }

        private decimal CalculateDeductions(decimal basicSalary)
        {
            // Standard deduction calculation
            // PF: 12% of basic, Professional Tax: 200, ESI (if applicable): 0.75%
            var pf = basicSalary * 0.12m;
            var professionalTax = 200m;
            var esi = basicSalary <= 21000 ? basicSalary * 0.0075m : 0m;
            return Math.Round(pf + professionalTax + esi, 2);
        }

        private string GeneratePayslipContent(Payroll payroll)
        {
            var content = new System.Text.StringBuilder();
            content.AppendLine("=====================================");
            content.AppendLine("           PAYSLIP");
            content.AppendLine("=====================================");
            content.AppendLine($"Employee: {payroll.Employee.FullName}");
            content.AppendLine($"Employee Code: {payroll.Employee.EmployeeCode}");
            content.AppendLine($"Department: {payroll.Employee.Department}");
            content.AppendLine($"Position: {payroll.Employee.Position}");
            content.AppendLine("-------------------------------------");
            content.AppendLine($"Pay Period: {payroll.PayPeriodStart:MMM dd} - {payroll.PayPeriodEnd:MMM dd, yyyy}");
            content.AppendLine($"Pay Date: {payroll.PayDate:MMM dd, yyyy}");
            content.AppendLine("-------------------------------------");
            content.AppendLine("EARNINGS:");
            content.AppendLine($"  Basic Salary:    ?{payroll.BasicSalary:N2}");
            content.AppendLine($"  Allowances:      ?{payroll.Allowances:N2}");
            content.AppendLine("-------------------------------------");
            content.AppendLine("DEDUCTIONS:");
            content.AppendLine($"  Total Deductions: ?{payroll.Deductions:N2}");
            content.AppendLine("=====================================");
            content.AppendLine($"NET SALARY:        ?{payroll.NetSalary:N2}");
            content.AppendLine("=====================================");
            return content.ToString();
        }
    }

    public class PayrollSummary
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalEmployees { get; set; }
        public decimal TotalBasicSalary { get; set; }
        public decimal TotalAllowances { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetSalary { get; set; }
        public int PaidCount { get; set; }
        public int PendingCount { get; set; }
        public int ProcessingCount { get; set; }
    }

    public class CreatePayrollViewModel
    {
        public int EmployeeId { get; set; }
        public DateTime PayPeriodStart { get; set; }
        public DateTime PayPeriodEnd { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal Allowances { get; set; }
        public decimal Deductions { get; set; }
        public DateTime PayDate { get; set; }
    }

    public class UpdatePayrollViewModel
    {
        public decimal BasicSalary { get; set; }
        public decimal Allowances { get; set; }
        public decimal Deductions { get; set; }
        public DateTime PayDate { get; set; }
        public string Status { get; set; } = "Pending";
    }

    public class ProcessPayrollViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime PayDate { get; set; }
        public string? Department { get; set; }
        public string? Notes { get; set; }
    }
}
