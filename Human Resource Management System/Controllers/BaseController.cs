using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Human_Resource_Management_System.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Skip authentication for LoginController
            if (context.Controller.GetType() != typeof(LoginController))
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    context.Result = RedirectToAction("Index", "Login");
                    return;
                }
            }
            base.OnActionExecuting(context);
        }
    }
}