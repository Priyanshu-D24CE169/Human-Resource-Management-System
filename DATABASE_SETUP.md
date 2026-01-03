# HRMS Database Setup Instructions

## Prerequisites
- .NET 8 SDK or .NET 10 SDK installed
- SQL Server or SQL Server LocalDB installed
- Visual Studio 2022 or later (or VS Code with C# extension)

## Step 1: Update Connection String
Update the connection string in `appsettings.json` to match your SQL Server instance:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HRMS_DB;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
}
```

For SQL Server (not LocalDB), use:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=HRMS_DB;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
}
```

## Step 2: Install Entity Framework Tools
Open Package Manager Console or Terminal and run:

```bash
dotnet tool install --global dotnet-ef
```

Or update if already installed:
```bash
dotnet tool update --global dotnet-ef
```

## Step 3: Create Initial Migration
In the project directory, run:

```bash
dotnet ef migrations add InitialCreate
```

This will create a `Migrations` folder with the initial database schema.

## Step 4: Update Database
Apply the migration to create the database:

```bash
dotnet ef database update
```

This will:
- Create the HRMS_DB database
- Create all tables (Users, Employees, Attendance, LeaveRequests, Salaries, etc.)
- Seed initial data (Admin role, Employee role, Default admin user)

## Step 5: Default Admin Credentials
After database creation, you can login with:

**Login ID:** ADMIN001
**Password:** Admin@123

**Admin Email:** admin@hrms.com

## Step 6: Configure Email Settings (Optional)
Update email settings in `appsettings.json`:

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

**For Gmail:**
1. Enable 2-Factor Authentication
2. Generate App Password: https://myaccount.google.com/apppasswords
3. Use the app password in the configuration

## Step 7: Run the Application
```bash
dotnet run
```

Or press F5 in Visual Studio.

The application will be available at:
- HTTPS: https://localhost:5001
- HTTP: http://localhost:5000

## Troubleshooting

### Error: "A connection was successfully established..."
- Ensure SQL Server is running
- Verify connection string is correct
- Check Windows Authentication is enabled

### Error: "The term 'dotnet-ef' is not recognized"
- Restart terminal/command prompt after installing dotnet-ef
- Ensure .NET SDK is properly installed

### Email Not Sending
- Check SMTP credentials
- For Gmail, ensure App Password is used (not regular password)
- Check firewall/antivirus settings

## Database Schema Overview

### Tables Created:
1. **AspNetUsers** - User authentication (customized with LoginId)
2. **AspNetRoles** - Roles (Admin, Employee)
3. **Employees** - Employee master data
4. **Attendances** - Daily attendance records
5. **LeaveRequests** - Leave applications
6. **Salaries** - Salary information
7. **CompanySettings** - Company configuration

### Sample Data Seeded:
- Admin Role
- Employee Role  
- Default Admin User (ADMIN001)
- Company Settings (Company Code: OI)

## Next Steps After Setup

1. **Login as Admin** with credentials above
2. **Create your first employee** from Employees menu
3. **Employee will receive email** with auto-generated credentials
4. **Employee can login** and must change password on first login
5. **Mark attendance** daily using Check-In/Check-Out
6. **Apply for leave** and get admin approval
7. **Admin can view reports** and manage all employees

## Features Overview

### Admin Features:
- Create/Edit employees
- Auto-generate Login IDs
- Manage salary information
- View all attendance records
- Approve/Reject leave requests
- Generate attendance reports

### Employee Features:
- View own profile
- Edit allowed fields
- Mark daily attendance (Check-In/Check-Out)
- Apply for leave
- View attendance history
- Change password

## Login ID Format
System auto-generates Login IDs in this format:
**[CompanyCode][FirstName Initial][LastName Initial][Year][Serial]**

Example: **OIJDODOA20230001**
- OI = Company Code (Odoo India)
- JD = John Doe
- 2023 = Year of Joining
- 0001 = Serial Number

## Security Features
- Password must be changed on first login
- Role-based authorization (Admin/Employee)
- Employees can only view their own data
- Salary info visible only to Admin
- Secure password requirements

## Support
For issues or questions, contact the development team.
