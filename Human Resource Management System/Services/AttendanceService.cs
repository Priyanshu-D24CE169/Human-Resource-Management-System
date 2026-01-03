using Human_Resource_Management_System.Data;
using Human_Resource_Management_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Human_Resource_Management_System.Services
{
    public interface IAttendanceService
    {
        Task<List<AttendanceViewModel>> GetAttendanceByDateAsync(DateTime date);
        Task<List<AttendanceViewModel>> GetAttendanceByEmployeeAsync(int employeeId);
        Task<List<AttendanceViewModel>> GetAttendanceByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<AttendanceViewModel?> GetAttendanceByIdAsync(int attendanceId);
        Task<Attendance?> GetAttendanceRecordAsync(int employeeId, DateTime date);
        Task<bool> MarkAttendanceAsync(int employeeId, DateTime date, TimeSpan? checkIn, TimeSpan? checkOut, string status, string? notes = null);
        Task<bool> UpdateAttendanceAsync(int attendanceId, TimeSpan? checkIn, TimeSpan? checkOut, string status, string? notes = null);
        Task<bool> DeleteAttendanceAsync(int attendanceId);
        Task<AttendanceSummary> GetAttendanceSummaryAsync(DateTime date);
        Task<List<Employee>> GetAllEmployeesAsync();
    }

    public class AttendanceService : IAttendanceService
    {
        private readonly HrmsDbContext _context;
        private readonly ILogger<AttendanceService> _logger;

        public AttendanceService(HrmsDbContext context, ILogger<AttendanceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<AttendanceViewModel>> GetAttendanceByDateAsync(DateTime date)
        {
            try
            {
                var employees = await _context.Employees
                    .Where(e => e.Status == "Active")
                    .ToListAsync();

                var attendances = await _context.Attendances
                    .Where(a => a.Date.Date == date.Date)
                    .ToListAsync();

                var result = new List<AttendanceViewModel>();

                foreach (var employee in employees)
                {
                    var attendance = attendances.FirstOrDefault(a => a.EmployeeId == employee.EmployeeId);
                    
                    result.Add(new AttendanceViewModel
                    {
                        AttendanceId = attendance?.AttendanceId ?? 0,
                        EmployeeId = employee.EmployeeId,
                        EmployeeName = employee.FullName,
                        EmployeeCode = employee.EmployeeCode,
                        Department = employee.Department,
                        Date = date,
                        CheckIn = attendance?.CheckIn ?? TimeSpan.Zero,
                        CheckOut = attendance?.CheckOut ?? TimeSpan.Zero,
                        Status = attendance?.Status ?? "Not Marked",
                        WorkingHours = attendance?.WorkingHours ?? 0,
                        Notes = attendance?.Notes
                    });
                }

                return result.OrderBy(a => a.EmployeeName).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting attendance for date: {date}");
                return new List<AttendanceViewModel>();
            }
        }

        public async Task<List<AttendanceViewModel>> GetAttendanceByEmployeeAsync(int employeeId)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(employeeId);
                if (employee == null) return new List<AttendanceViewModel>();

                var attendances = await _context.Attendances
                    .Where(a => a.EmployeeId == employeeId)
                    .OrderByDescending(a => a.Date)
                    .ToListAsync();

                return attendances.Select(a => new AttendanceViewModel
                {
                    AttendanceId = a.AttendanceId,
                    EmployeeId = a.EmployeeId,
                    EmployeeName = employee.FullName,
                    EmployeeCode = employee.EmployeeCode,
                    Department = employee.Department,
                    Date = a.Date,
                    CheckIn = a.CheckIn ?? TimeSpan.Zero,
                    CheckOut = a.CheckOut ?? TimeSpan.Zero,
                    Status = a.Status,
                    WorkingHours = a.WorkingHours,
                    Notes = a.Notes
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting attendance for employee: {employeeId}");
                return new List<AttendanceViewModel>();
            }
        }

        public async Task<List<AttendanceViewModel>> GetAttendanceByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var attendances = await _context.Attendances
                    .Include(a => a.Employee)
                    .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date)
                    .OrderByDescending(a => a.Date)
                    .ThenBy(a => a.Employee.FirstName)
                    .ToListAsync();

                return attendances.Select(a => new AttendanceViewModel
                {
                    AttendanceId = a.AttendanceId,
                    EmployeeId = a.EmployeeId,
                    EmployeeName = a.Employee.FullName,
                    EmployeeCode = a.Employee.EmployeeCode,
                    Department = a.Employee.Department,
                    Date = a.Date,
                    CheckIn = a.CheckIn ?? TimeSpan.Zero,
                    CheckOut = a.CheckOut ?? TimeSpan.Zero,
                    Status = a.Status,
                    WorkingHours = a.WorkingHours,
                    Notes = a.Notes
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting attendance for date range: {startDate} - {endDate}");
                return new List<AttendanceViewModel>();
            }
        }

        public async Task<AttendanceViewModel?> GetAttendanceByIdAsync(int attendanceId)
        {
            try
            {
                var attendance = await _context.Attendances
                    .Include(a => a.Employee)
                    .FirstOrDefaultAsync(a => a.AttendanceId == attendanceId);

                if (attendance == null) return null;

                return new AttendanceViewModel
                {
                    AttendanceId = attendance.AttendanceId,
                    EmployeeId = attendance.EmployeeId,
                    EmployeeName = attendance.Employee.FullName,
                    EmployeeCode = attendance.Employee.EmployeeCode,
                    Department = attendance.Employee.Department,
                    Date = attendance.Date,
                    CheckIn = attendance.CheckIn ?? TimeSpan.Zero,
                    CheckOut = attendance.CheckOut ?? TimeSpan.Zero,
                    Status = attendance.Status,
                    WorkingHours = attendance.WorkingHours,
                    Notes = attendance.Notes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting attendance by ID: {attendanceId}");
                return null;
            }
        }

        public async Task<Attendance?> GetAttendanceRecordAsync(int employeeId, DateTime date)
        {
            try
            {
                return await _context.Attendances
                    .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date.Date == date.Date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting attendance record for employee {employeeId} on {date}");
                return null;
            }
        }

        public async Task<bool> MarkAttendanceAsync(int employeeId, DateTime date, TimeSpan? checkIn, TimeSpan? checkOut, string status, string? notes = null)
        {
            try
            {
                // Check if attendance already exists for this employee and date
                var existingAttendance = await GetAttendanceRecordAsync(employeeId, date);
                
                if (existingAttendance != null)
                {
                    // Update existing record
                    existingAttendance.CheckIn = checkIn;
                    existingAttendance.CheckOut = checkOut;
                    existingAttendance.Status = status;
                    existingAttendance.Notes = notes;
                    existingAttendance.WorkingHours = CalculateWorkingHours(checkIn, checkOut);
                }
                else
                {
                    // Create new record
                    var attendance = new Attendance
                    {
                        EmployeeId = employeeId,
                        Date = date.Date,
                        CheckIn = checkIn,
                        CheckOut = checkOut,
                        Status = status,
                        Notes = notes,
                        WorkingHours = CalculateWorkingHours(checkIn, checkOut),
                        CreatedDate = DateTime.Now
                    };
                    _context.Attendances.Add(attendance);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Attendance marked for employee {employeeId} on {date}: {status}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking attendance for employee {employeeId} on {date}");
                return false;
            }
        }

        public async Task<bool> UpdateAttendanceAsync(int attendanceId, TimeSpan? checkIn, TimeSpan? checkOut, string status, string? notes = null)
        {
            try
            {
                var attendance = await _context.Attendances.FindAsync(attendanceId);
                if (attendance == null) return false;

                attendance.CheckIn = checkIn;
                attendance.CheckOut = checkOut;
                attendance.Status = status;
                attendance.Notes = notes;
                attendance.WorkingHours = CalculateWorkingHours(checkIn, checkOut);

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Attendance updated: {attendanceId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating attendance: {attendanceId}");
                return false;
            }
        }

        public async Task<bool> DeleteAttendanceAsync(int attendanceId)
        {
            try
            {
                var attendance = await _context.Attendances.FindAsync(attendanceId);
                if (attendance == null) return false;

                _context.Attendances.Remove(attendance);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Attendance deleted: {attendanceId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting attendance: {attendanceId}");
                return false;
            }
        }

        public async Task<AttendanceSummary> GetAttendanceSummaryAsync(DateTime date)
        {
            try
            {
                var totalEmployees = await _context.Employees.CountAsync(e => e.Status == "Active");
                var attendances = await _context.Attendances
                    .Where(a => a.Date.Date == date.Date)
                    .ToListAsync();

                return new AttendanceSummary
                {
                    Date = date,
                    TotalEmployees = totalEmployees,
                    PresentCount = attendances.Count(a => a.Status == "Present"),
                    AbsentCount = attendances.Count(a => a.Status == "Absent"),
                    OnLeaveCount = attendances.Count(a => a.Status == "Leave"),
                    HalfDayCount = attendances.Count(a => a.Status == "Half Day"),
                    NotMarkedCount = totalEmployees - attendances.Count,
                    AverageWorkingHours = attendances.Where(a => a.Status == "Present").Average(a => (double?)a.WorkingHours) ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting attendance summary for date: {date}");
                return new AttendanceSummary { Date = date };
            }
        }

        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            try
            {
                return await _context.Employees
                    .Where(e => e.Status == "Active")
                    .OrderBy(e => e.FirstName)
                    .ThenBy(e => e.LastName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all employees");
                return new List<Employee>();
            }
        }

        private decimal CalculateWorkingHours(TimeSpan? checkIn, TimeSpan? checkOut)
        {
            if (!checkIn.HasValue || !checkOut.HasValue) return 0;
            if (checkOut.Value <= checkIn.Value) return 0;

            var duration = checkOut.Value - checkIn.Value;
            return Math.Round((decimal)duration.TotalHours, 2);
        }
    }

    public class AttendanceSummary
    {
        public DateTime Date { get; set; }
        public int TotalEmployees { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int OnLeaveCount { get; set; }
        public int HalfDayCount { get; set; }
        public int NotMarkedCount { get; set; }
        public double AverageWorkingHours { get; set; }
    }
}
