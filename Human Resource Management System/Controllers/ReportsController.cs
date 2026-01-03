using Microsoft.AspNetCore.Mvc;

namespace Human_Resource_Management_System.Controllers
{
    public class ReportsController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}