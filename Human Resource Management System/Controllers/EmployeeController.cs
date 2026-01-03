using Microsoft.AspNetCore.Mvc;
using Human_Resource_Management_System.Models;

namespace Human_Resource_Management_System.Controllers
{
    public class EmployeeController : BaseController
    {
        public IActionResult Index()
        {
            // Sample data - in real application, this would come from database
            var employees = new List<EmployeeViewModel>
            {
                new EmployeeViewModel { EmployeeId = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@company.com", Department = "IT", Position = "Software Developer", HireDate = DateTime.Parse("2023-01-15"), Salary = 75000, Status = "Active" },
                new EmployeeViewModel { EmployeeId = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@company.com", Department = "HR", Position = "HR Manager", HireDate = DateTime.Parse("2022-08-20"), Salary = 85000, Status = "Active" },
                new EmployeeViewModel { EmployeeId = 3, FirstName = "Mike", LastName = "Johnson", Email = "mike.johnson@company.com", Department = "Finance", Position = "Accountant", HireDate = DateTime.Parse("2023-03-10"), Salary = 65000, Status = "Active" },
                new EmployeeViewModel { EmployeeId = 4, FirstName = "Sarah", LastName = "Wilson", Email = "sarah.wilson@company.com", Department = "Marketing", Position = "Marketing Specialist", HireDate = DateTime.Parse("2023-05-22"), Salary = 70000, Status = "Active" },
                new EmployeeViewModel { EmployeeId = 5, FirstName = "Robert", LastName = "Brown", Email = "robert.brown@company.com", Department = "IT", Position = "System Administrator", HireDate = DateTime.Parse("2022-11-08"), Salary = 80000, Status = "Active" }
            };

            return View(employees);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(EmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                // TODO: Save employee to database
                TempData["Success"] = "Employee created successfully!";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public IActionResult Edit(int id)
        {
            // TODO: Get employee from database by id
            var employee = new EmployeeViewModel
            {
                EmployeeId = id,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@company.com",
                Department = "IT",
                Position = "Software Developer",
                HireDate = DateTime.Parse("2023-01-15"),
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
                // TODO: Update employee in database
                TempData["Success"] = "Employee updated successfully!";
                return RedirectToAction("Index");
            }
            return View(model);
        }
    }
}