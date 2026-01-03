# HRMS - HR Management System

A comprehensive **Human Resource Management System** built with **ASP.NET Core MVC**, **.NET 8/10**, **Entity Framework Core**, and **SQL Server**.

## ?? Features

### Authentication & Authorization
- ? Custom Login with **Auto-Generated Login IDs** (not email-based)
- ? Role-based access control (Admin & Employee)
- ? Force password change on first login
- ? Secure password requirements

### Employee Management
- ? Admin can create employee accounts
- ? Auto-generated Login ID format: `[CompanyCode][Initials][Year][Serial]`
- ? Auto-generated temporary passwords sent via email
- ? Complete employee profile management
- ? Document uploads (Profile photo, Resume)
- ? Bank and personal information

### Dashboard
- ? Employee cards with status indicators
  - ?? Green = Present
  - ?? Blue = On Leave
  - ?? Yellow = Absent
- ? Real-time attendance status
- ? Quick statistics
- ? Clickable cards for employee profiles

### Attendance Management
- ? Daily Check-In / Check-Out system
- ? Automatic work hours calculation
- ? Extra hours tracking
- ? Attendance history
- ? Admin: View all employees' attendance
- ? Employee: View only own attendance
- ? Date-wise attendance reports

### Leave Management
- ? Three types of leave:
  - Paid Leave
  - Sick Leave
  - Unpaid Leave
- ? Leave application with attachments
- ? Admin approval/rejection workflow
- ? Email notifications on leave status
- ? Automatic attendance marking for approved leaves

### Salary Management (Admin Only)
- ? Monthly and yearly wage tracking
- ? Salary component breakdown:
  - Basic (50%)
  - HRA (20%)
  - PF (12%)
  - Allowances
  - Deductions
  - Professional Tax
- ? Per-day wage calculation
- ? Attendance-based salary computation

### Profile Management
- ? Tabbed profile view:
  - Resume
  - Private Info
  - Salary Info (Admin only)
  - Security
- ? Employees can edit own profile
- ? Admin can edit all profiles
- ? Change password functionality

### Email Notifications
- ? Send credentials on employee creation
- ? Leave approval/rejection notifications
- ? SMTP email service integration

## ??? Technology Stack

- **Framework:** ASP.NET Core MVC (.NET 8/10)
- **ORM:** Entity Framework Core (Code First)
- **Database:** Microsoft SQL Server
- **Authentication:** ASP.NET Core Identity (Customized)
- **UI Framework:** Bootstrap 5
- **Icons:** Bootstrap Icons
- **Email:** MailKit (SMTP)

## ?? Prerequisites

- .NET 8 SDK or .NET 10 SDK
- SQL Server 2019+ or SQL Server LocalDB
- Visual Studio 2022 / VS Code / Rider
- SMTP Email Account (Gmail recommended)

## ?? Installation & Setup

### 1. Clone the Repository
```bash
git clone <repository-url>
cd hrms
```

### 2. Update Connection String
Edit `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HRMS_DB;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
}
```

### 3. Update Email Settings
Edit `appsettings.json`:
```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": "587",
  "SenderEmail": "your-email@gmail.com",
  "SenderName": "HRMS System",
  "Username": "your-email@gmail.com",
  "Password": "your-app-password"
}
```

### 4. Install Dependencies
```bash
dotnet restore
```

### 5. Create Database
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 6. Run the Application
```bash
dotnet run
```

Navigate to: `https://localhost:5001` or `http://localhost:5000`

## ?? Default Admin Credentials

**Login ID:** `ADMIN001`  
**Password:** `Admin@123`

## ?? Project Structure

```
hrms/
??? Controllers/
?   ??? AccountController.cs
?   ??? DashboardController.cs
?   ??? EmployeeController.cs
?   ??? AttendanceController.cs
?   ??? LeaveController.cs
??? Models/
?   ??? ApplicationUser.cs
?   ??? Employee.cs
?   ??? Attendance.cs
?   ??? LeaveRequest.cs
?   ??? Salary.cs
?   ??? CompanySettings.cs
??? ViewModels/
?   ??? LoginViewModel.cs
?   ??? DashboardViewModel.cs
?   ??? EmployeeViewModel.cs
?   ??? AttendanceViewModel.cs
?   ??? LeaveViewModel.cs
??? Views/
?   ??? Account/
?   ??? Dashboard/
?   ??? Employee/
?   ??? Attendance/
?   ??? Leave/
?   ??? Shared/
??? Data/
?   ??? ApplicationDbContext.cs
??? Services/
?   ??? IEmailService.cs
?   ??? EmailService.cs
?   ??? EmployeeService.cs
??? wwwroot/
?   ??? css/
?   ??? js/
?   ??? uploads/
??? Program.cs
```

## ?? Security Features

- Password complexity requirements
- Role-based authorization policies
- CSRF protection
- Secure cookie authentication
- First-login password change enforcement
- Access control based on roles

## ?? Database Schema

### Main Tables
- **AspNetUsers** - User accounts with custom LoginId
- **AspNetRoles** - Admin & Employee roles
- **Employees** - Employee master data
- **Attendances** - Daily attendance records
- **LeaveRequests** - Leave applications
- **Salaries** - Salary information
- **CompanySettings** - Company configuration

## ?? Key Functionalities

### For Admin:
1. Create employee accounts with auto-generated credentials
2. Manage all employee information
3. View and edit salary information
4. Approve/reject leave requests
5. View attendance reports for all employees
6. Generate reports and analytics

### For Employee:
1. Login with auto-generated credentials
2. Change password on first login
3. Mark daily attendance (Check-In/Check-Out)
4. Apply for leave with attachments
5. View own attendance history
6. Update personal profile information

## ?? Email Configuration (Gmail)

1. Enable 2-Factor Authentication on your Gmail account
2. Generate App Password: https://myaccount.google.com/apppasswords
3. Use the app password in `appsettings.json`

## ?? Login ID Auto-Generation

Format: `[CompanyCode][FirstInitial][LastInitial][Year][Serial]`

**Example:** `OIJD20230001`
- **OI** = Odoo India (Company Code)
- **JD** = John Doe (Initials)
- **2023** = Year of Joining
- **0001** = Serial Number

## ?? Usage Workflow

1. **Admin logs in** ? Creates new employee
2. **System generates** ? Login ID & Password
3. **Email sent to employee** ? With credentials
4. **Employee logs in** ? Must change password
5. **Employee marks attendance** ? Daily Check-In/Check-Out
6. **Employee applies leave** ? With reason & documents
7. **Admin approves leave** ? Email sent to employee
8. **System marks attendance** ? As "On Leave" for approved dates

## ?? Troubleshooting

### Database Connection Issues
- Ensure SQL Server is running
- Verify connection string in `appsettings.json`
- Check Windows Authentication is enabled

### Email Not Sending
- Verify SMTP credentials
- Use App Password for Gmail (not regular password)
- Check firewall settings

### Migration Errors
```bash
dotnet ef database drop -f
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## ?? Contributing

Contributions are welcome! Please follow these steps:
1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## ?? License

This project is licensed under the MIT License.

## ????? Developer

Built with ?? for HR Management

## ?? Support

For issues or questions:
- Create an issue on GitHub
- Contact the development team

---

**Happy HR Management! ??**
