using Microsoft.AspNetCore.Mvc;
using Human_Resource_Management_System.Attributes;

namespace Human_Resource_Management_System.Controllers
{
    [AdminAuthorize]
    public class ReportsController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}