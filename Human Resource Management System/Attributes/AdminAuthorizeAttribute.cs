using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Human_Resource_Management_System.Attributes
{
    /// <summary>
    /// Authorization attribute that restricts access to Admin users only.
    /// Redirects non-admin users to appropriate dashboards.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Check if action has AllowAnonymousRegistration attribute
            var actionDescriptor = context.ActionDescriptor;
            var hasAllowAnonymous = actionDescriptor.EndpointMetadata
                .Any(m => m is AllowAnonymousRegistrationAttribute);
            
            if (hasAllowAnonymous)
            {
                base.OnActionExecuting(context);
                return;
            }

            var httpContext = context.HttpContext;
            var userId = httpContext.Session.GetString("UserId");
            var userRole = httpContext.Session.GetString("UserRole");

            // Check if user is logged in
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
                return;
            }

            // Check if user is Admin
            if (userRole != "Admin")
            {
                // Redirect non-admin users to their appropriate dashboard
                if (userRole == "Employee")
                {
                    context.Result = new RedirectToActionResult("Index", "EmployeeDashboard", null);
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
