using Microsoft.AspNetCore.Mvc;
using Human_Resource_Management_System.Models;
using Human_Resource_Management_System.Attributes;

namespace Human_Resource_Management_System.Controllers
{
    [AdminAuthorize]
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