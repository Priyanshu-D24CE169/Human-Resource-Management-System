using Microsoft.AspNetCore.Mvc.Filters;

namespace Human_Resource_Management_System.Attributes
{
    /// <summary>
    /// Marks an action method to allow anonymous access for employee registration.
    /// This bypasses the AdminAuthorize attribute on the controller.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AllowAnonymousRegistrationAttribute : Attribute, IFilterMetadata
    {
    }
}
