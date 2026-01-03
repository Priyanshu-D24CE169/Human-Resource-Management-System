using Microsoft.AspNetCore.Identity;

namespace hrms.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string LoginId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool MustChangePassword { get; set; } = true;
        public int? EmployeeId { get; set; }
        public Employee? Employee { get; set; }
    }
}
