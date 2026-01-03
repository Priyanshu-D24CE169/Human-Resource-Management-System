using hrms.Models;

namespace hrms.ViewModels
{
    public class DashboardViewModel
    {
        public List<EmployeeCardViewModel> Employees { get; set; } = new List<EmployeeCardViewModel>();
        public string CurrentUserRole { get; set; } = string.Empty;
        public int? CurrentEmployeeId { get; set; }
        public UserCheckInStatus? CurrentUserCheckInStatus { get; set; }
    }

    public class EmployeeCardViewModel
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string? ProfilePhotoPath { get; set; }
        public string? Designation { get; set; }
        public string? Department { get; set; }
        public DateTime JoiningDate { get; set; }
        public string Status { get; set; } = "Absent"; // Present, OnLeave, Absent
        public string StatusIcon { get; set; } = "circle-fill";
        public string StatusColor { get; set; } = "warning";
        public string StatusTooltip { get; set; } = "Absent";
        
        // Legacy property for backward compatibility
        public string StatusColorLegacy => Status switch
        {
            "Present" => "success",
            "OnLeave" => "info",
            _ => "warning"
        };
    }

    public class UserCheckInStatus
    {
        public bool HasCheckedIn { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public bool CanCheckOut { get; set; }
        
        public string StatusIcon => HasCheckedIn ? "circle-fill" : "circle-fill";
        public string StatusColor => HasCheckedIn ? "success" : "danger";
        public string StatusText => HasCheckedIn ? "Checked In" : "Check In Required";
        public string StatusTooltip => HasCheckedIn 
            ? $"Checked in at {CheckInTime:HH:mm}" 
            : "Click to check in";
    }
}
