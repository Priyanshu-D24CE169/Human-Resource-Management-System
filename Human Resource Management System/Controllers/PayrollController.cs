using Microsoft.AspNetCore.Mvc;
using Human_Resource_Management_System.Models;
using Human_Resource_Management_System.Attributes;

namespace Human_Resource_Management_System.Controllers
{
    [AdminAuthorize]
    public class PayrollController : BaseController
    {
        public IActionResult Index()
        {
            // Sample data - in real application, this would come from database
            var payroll = new List<PayrollViewModel>
            {
                new PayrollViewModel { PayrollId = 1, EmployeeId = 1, EmployeeName = "John Doe", PayPeriodStart = DateTime.Parse("2024-01-01"), PayPeriodEnd = DateTime.Parse("2024-01-31"), BasicSalary = 75000, Allowances = 5000, Deductions = 8000, NetSalary = 72000, PayDate = DateTime.Parse("2024-02-01"), Status = "Paid" },
                new PayrollViewModel { PayrollId = 2, EmployeeId = 2, EmployeeName = "Jane Smith", PayPeriodStart = DateTime.Parse("2024-01-01"), PayPeriodEnd = DateTime.Parse("2024-01-31"), BasicSalary = 85000, Allowances = 6000, Deductions = 9000, NetSalary = 82000, PayDate = DateTime.Parse("2024-02-01"), Status = "Paid" },
                new PayrollViewModel { PayrollId = 3, EmployeeId = 3, EmployeeName = "Mike Johnson", PayPeriodStart = DateTime.Parse("2024-01-01"), PayPeriodEnd = DateTime.Parse("2024-01-31"), BasicSalary = 65000, Allowances = 4000, Deductions = 7000, NetSalary = 62000, PayDate = DateTime.Parse("2024-02-01"), Status = "Paid" },
                new PayrollViewModel { PayrollId = 4, EmployeeId = 4, EmployeeName = "Sarah Wilson", PayPeriodStart = DateTime.Parse("2024-01-01"), PayPeriodEnd = DateTime.Parse("2024-01-31"), BasicSalary = 70000, Allowances = 4500, Deductions = 7500, NetSalary = 67000, PayDate = DateTime.Parse("2024-02-01"), Status = "Processing" },
                new PayrollViewModel { PayrollId = 5, EmployeeId = 5, EmployeeName = "Robert Brown", PayPeriodStart = DateTime.Parse("2024-01-01"), PayPeriodEnd = DateTime.Parse("2024-01-31"), BasicSalary = 80000, Allowances = 5500, Deductions = 8500, NetSalary = 77000, PayDate = DateTime.Parse("2024-02-01"), Status = "Pending" }
            };

            return View(payroll);
        }
    }
}