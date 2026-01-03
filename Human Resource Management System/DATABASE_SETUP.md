# Database Migration Commands

## Create Migration
```bash
dotnet ef migrations add InitialEmployeeManagement
```

## Update Database
```bash
dotnet ef database update
```

## Drop Database (if needed for testing)
```bash
dotnet ef database drop
```

## Create Migration for Employee Code Updates
```bash
dotnet ef migrations add AddEmployeeCodeAndRegistration
```

## SQL Commands for Manual Database Setup (if needed)

### Create Database
```sql
CREATE DATABASE IF NOT EXISTS hrms_database;
USE hrms_database;
```

### Check Tables
```sql
SHOW TABLES;
DESCRIBE Employees;
DESCRIBE Users;
```

### Manual Employee Code Update (if needed)
```sql
UPDATE Employees 
SET EmployeeCode = CONCAT('OI', 
    UPPER(LEFT(FirstName, 2)), 
    UPPER(LEFT(LastName, 2)), 
    YEAR(HireDate), 
    LPAD(EmployeeId, 4, '0')
) 
WHERE EmployeeCode IS NULL OR EmployeeCode = '';
```

### Check Employee Codes
```sql
SELECT EmployeeId, EmployeeCode, FirstName, LastName, HireDate, IsRegistrationComplete 
FROM Employees 
ORDER BY EmployeeId;
```

### Check Admin User
```sql
SELECT UserId, Email, FirstName, LastName, Role, IsActive 
FROM Users 
WHERE Email = 'admin@hrms.com';
```

## Testing Commands

### Test Email Configuration
- Check SMTP settings in appsettings.json
- Verify Gmail app password is correct
- Test with a real email address

### Test Employee Creation Flow
1. Login as admin (admin@hrms.com / admin123)
2. Navigate to Employee Management
3. Create new employee with real email
4. Check email delivery
5. Complete registration process
6. Test employee login

### Common Issues and Solutions

#### Email Not Sending
- Check Gmail app password
- Verify SMTP settings
- Check firewall/antivirus blocking
- Review application logs

#### Database Connection Issues
- Ensure MySQL is running on 192.168.0.114:3306
- Check network connectivity
- Verify database credentials
- Test connection string

#### Employee Code Generation Issues
- Check database for existing codes
- Verify employee creation logic
- Run migration to update existing employees