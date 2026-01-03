using Microsoft.AspNetCore.Mvc;
using Human_Resource_Management_System.Models;

namespace Human_Resource_Management_System.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}