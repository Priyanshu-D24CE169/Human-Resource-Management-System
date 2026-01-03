using Microsoft.AspNetCore.Mvc;
using Human_Resource_Management_System.Models;

namespace Human_Resource_Management_System.Controllers
{
    public class EmployeeController : Controller
    {
        public IActionResult Index()
        {
            // Sample data - in real application, this would come from database
            var employees = new List<EmployeeViewModel>
            {
                new EmployeeViewModel { Id = "EMP001", Name = "John Doe", Email = "john.doe@company.com", Department = "IT", Position = "Software Developer", JoinDate = DateTime.Parse("2023-01-15"), Salary = 75000, Status = "Active" },
                new EmployeeViewModel { Id = "EMP002", Name = "Jane Smith", Email = "jane.smith@company.com", Department = "HR", Position = "HR Manager", JoinDate = DateTime.Parse("2022-08-20"), Salary = 85000, Status = "Active" },
                new EmployeeViewModel { Id = "EMP003", Name = "Mike Johnson", Email = "mike.johnson@company.com", Department = "Finance", Position = "Accountant", JoinDate = DateTime.Parse("2023-03-10"), Salary = 65000, Status = "Active" },
                new EmployeeViewModel { Id = "EMP004", Name = "Sarah Wilson", Email = "sarah.wilson@company.com", Department = "IT", Position = "UI/UX Designer", JoinDate = DateTime.Parse("2023-02-01"), Salary = 70000, Status = "Active" },
                new EmployeeViewModel { Id = "EMP005", Name = "Robert Brown", Email = "robert.brown@company.com", Department = "Sales", Position = "Sales Executive", JoinDate = DateTime.Parse("2022-11-15"), Salary = 60000, Status = "On Leave" }
            };
            
            return View(employees);
        }

        public IActionResult Create()
        {
            return View(new EmployeeViewModel());
        }

        [HttpPost]
        public IActionResult Create(EmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                // In real application, save to database
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public IActionResult Edit(string id)
        {
            // In real application, fetch from database
            var employee = new EmployeeViewModel 
            { 
                Id = id, 
                Name = "John Doe", 
                Email = "john.doe@company.com", 
                Department = "IT", 
                Position = "Software Developer", 
                JoinDate = DateTime.Parse("2023-01-15"), 
                Salary = 75000, 
                Status = "Active" 
            };
            return View(employee);
        }

        [HttpPost]
        public IActionResult Edit(EmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                // In real application, update in database
                return RedirectToAction("Index");
            }
            return View(model);
        }
    }
}