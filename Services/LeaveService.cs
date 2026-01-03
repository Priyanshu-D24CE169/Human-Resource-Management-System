using hrms.Data;
using hrms.Models;
using Microsoft.EntityFrameworkCore;

namespace hrms.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LeaveService> _logger;

        public LeaveService(ApplicationDbContext context, ILogger<LeaveService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task InitializeEmployeeLeaveBalancesAsync()
        {
            var employees = await _context.Employees
                .Where(e => e.IsActive && (e.PaidTimeOffBalance == 0 || e.SickTimeOffBalance == 0))
                .ToListAsync();

            foreach (var employee in employees)
            {
                // Set default leave balances if not already set
                if (employee.PaidTimeOffBalance == 0)
                {
                    employee.PaidTimeOffBalance = GetAnnualPTOAllocation(employee);
                }

                if (employee.SickTimeOffBalance == 0)
                {
                    employee.SickTimeOffBalance = GetAnnualSickLeaveAllocation(employee);
                }

                // Reset the last reset date to current year
                employee.LeaveBalanceLastReset = new DateTime(DateTime.Now.Year, 1, 1);
            }

            if (employees.Any())
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Initialized leave balances for {employees.Count} employees");
            }
        }

        public async Task<bool> HasSufficientLeaveBalanceAsync(int employeeId, string leaveType, int days)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                return false;

            // Check if annual reset is needed
            await CheckAndPerformAnnualResetAsync(employee);

            return leaveType.ToLower() switch
            {
                "paidleave" => employee.AvailablePTO >= days,
                "sickleave" => employee.AvailableSickDays >= days,
                "unpaidleave" => true, // Unpaid leave doesn't affect balance
                _ => false
            };
        }

        public async Task DeductLeaveBalanceAsync(int employeeId, string leaveType, int days)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                return;

            // Check if annual reset is needed
            await CheckAndPerformAnnualResetAsync(employee);

            switch (leaveType.ToLower())
            {
                case "paidleave":
                    employee.CurrentYearPTOUsed += days;
                    break;
                case "sickleave":
                    employee.CurrentYearSickUsed += days;
                    break;
                case "unpaidleave":
                    // No deduction for unpaid leave
                    break;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Deducted {days} days of {leaveType} for employee {employeeId}");
        }

        public async Task RestoreLeaveBalanceAsync(int employeeId, string leaveType, int days)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                return;

            switch (leaveType.ToLower())
            {
                case "paidleave":
                    employee.CurrentYearPTOUsed = Math.Max(0, employee.CurrentYearPTOUsed - days);
                    break;
                case "sickleave":
                    employee.CurrentYearSickUsed = Math.Max(0, employee.CurrentYearSickUsed - days);
                    break;
                case "unpaidleave":
                    // No restoration for unpaid leave
                    break;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Restored {days} days of {leaveType} for employee {employeeId}");
        }

        public async Task ResetAnnualLeaveBalancesAsync()
        {
            var employees = await _context.Employees
                .Where(e => e.IsActive)
                .ToListAsync();

            foreach (var employee in employees)
            {
                // Reset used days to 0
                employee.CurrentYearPTOUsed = 0;
                employee.CurrentYearSickUsed = 0;

                // Update allocation based on tenure
                employee.PaidTimeOffBalance = GetAnnualPTOAllocation(employee);
                employee.SickTimeOffBalance = GetAnnualSickLeaveAllocation(employee);

                // Update reset date
                employee.LeaveBalanceLastReset = new DateTime(DateTime.Now.Year, 1, 1);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Reset annual leave balances for {employees.Count} employees");
        }

        public async Task<Employee> GetEmployeeLeaveBalanceAsync(int employeeId)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee != null)
            {
                // Check if annual reset is needed
                await CheckAndPerformAnnualResetAsync(employee);
            }

            return employee!;
        }

        private async Task CheckAndPerformAnnualResetAsync(Employee employee)
        {
            var currentYear = DateTime.Now.Year;
            var lastResetYear = employee.LeaveBalanceLastReset.Year;

            if (currentYear > lastResetYear)
            {
                // Reset for new year
                employee.CurrentYearPTOUsed = 0;
                employee.CurrentYearSickUsed = 0;
                employee.PaidTimeOffBalance = GetAnnualPTOAllocation(employee);
                employee.SickTimeOffBalance = GetAnnualSickLeaveAllocation(employee);
                employee.LeaveBalanceLastReset = new DateTime(currentYear, 1, 1);

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Performed annual leave balance reset for employee {employee.EmployeeId}");
            }
        }

        private int GetAnnualPTOAllocation(Employee employee)
        {
            // Calculate PTO based on tenure
            var yearsOfService = DateTime.Now.Year - employee.JoiningDate.Year;

            return yearsOfService switch
            {
                < 1 => 12,  // Probationary period
                < 3 => 18,  // 1-2 years
                < 5 => 24,  // 3-4 years  
                < 10 => 30, // 5-9 years
                _ => 36     // 10+ years
            };
        }

        private int GetAnnualSickLeaveAllocation(Employee employee)
        {
            // Standard sick leave allocation
            return 12; // All employees get 12 sick days per year
        }
    }
}