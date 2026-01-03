using hrms.Models;

namespace hrms.Services
{
    public interface ILeaveService
    {
        Task InitializeEmployeeLeaveBalancesAsync();
        Task<bool> HasSufficientLeaveBalanceAsync(int employeeId, string leaveType, int days);
        Task DeductLeaveBalanceAsync(int employeeId, string leaveType, int days);
        Task RestoreLeaveBalanceAsync(int employeeId, string leaveType, int days);
        Task ResetAnnualLeaveBalancesAsync();
        Task<Employee> GetEmployeeLeaveBalanceAsync(int employeeId);
    }
}