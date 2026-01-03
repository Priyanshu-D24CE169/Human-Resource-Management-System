using Microsoft.AspNetCore.Mvc;
using Human_Resource_Management_System.Models;

namespace Human_Resource_Management_System.Controllers
{
    public class DashboardController : BaseController
    {
        public IActionResult Index()
        {
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            return View();
        }
    }
}