using Microsoft.AspNetCore.Mvc;
using Human_Resource_Management_System.Models;

namespace Human_Resource_Management_System.Controllers
{
    public class PayrollController : Controller
    {
        public IActionResult Index()
        {
            // Sample data - in real application, this would come from database
            var payroll = new List<PayrollViewModel>
            {
                new PayrollViewModel { EmployeeId = "EMP001", EmployeeName = "John Doe", BasicSalary = 75000, Allowances = 10000, Deductions = 5000, NetSalary = 80000, PayDate = DateTime.Parse("2024-03-01"), Status = "Paid" },
                new PayrollViewModel { EmployeeId = "EMP002", EmployeeName = "Jane Smith", BasicSalary = 85000, Allowances = 12000, Deductions = 7000, NetSalary = 90000, PayDate = DateTime.Parse("2024-03-01"), Status = "Paid" },
                new PayrollViewModel { EmployeeId = "EMP003", EmployeeName = "Mike Johnson", BasicSalary = 65000, Allowances = 8000, Deductions = 4000, NetSalary = 69000, PayDate = DateTime.Parse("2024-03-01"), Status = "Processing" },
                new PayrollViewModel { EmployeeId = "EMP004", EmployeeName = "Sarah Wilson", BasicSalary = 70000, Allowances = 9000, Deductions = 4500, NetSalary = 74500, PayDate = DateTime.Parse("2024-03-01"), Status = "Paid" },
                new PayrollViewModel { EmployeeId = "EMP005", EmployeeName = "Robert Brown", BasicSalary = 60000, Allowances = 7000, Deductions = 3000, NetSalary = 64000, PayDate = DateTime.Parse("2024-03-01"), Status = "Pending" }
            };
            
            return View(payroll);
        }
    }
}