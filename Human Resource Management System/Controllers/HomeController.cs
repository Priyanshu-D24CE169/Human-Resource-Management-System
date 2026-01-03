using System.Diagnostics;
using Human_Resource_Management_System.Models;
using Microsoft.AspNetCore.Mvc;

namespace Human_Resource_Management_System.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            // Redirect to appropriate dashboard based on role
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (userRole == "Admin")
            {
                return RedirectToAction("Index", "Dashboard");
            }
            else if (userRole == "Employee")
            {
                return RedirectToAction("Index", "EmployeeDashboard");
            }
            
            return RedirectToAction("Index", "Login");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
