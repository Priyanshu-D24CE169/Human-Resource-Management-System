using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Human_Resource_Management_System.Attributes
{
    /// <summary>
    /// Authorization attribute that restricts access to Employee users only.
    /// Redirects admin users to their dashboard.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class EmployeeAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var userId = httpContext.Session.GetString("UserId");
            var userRole = httpContext.Session.GetString("UserRole");

            // Check if user is logged in
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
                return;
            }

            // Check if user is Employee
            if (userRole != "Employee")
            {
                // Redirect admin users to their dashboard
                if (userRole == "Admin")
                {
                    context.Result = new RedirectToActionResult("Index", "Dashboard", null);
                }
                else
                {
                    context.Result = new RedirectToActionResult("Index", "Login", null);
                }
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
