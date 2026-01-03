# ?? Database Schema Fix Instructions

## Problem Identified ?
The error "Error creating employee. Please try again." is caused by a **database schema mismatch**. The application is trying to access columns (`Address`, `EmployeeCode`, etc.) that don't exist in your current MySQL database table.

## Quick Fix Steps

### Option 1: Restart Application (Recommended - Easiest)
1. **Stop the current application** (Stop debugging in Visual Studio)
2. **Restart the application** (`F5` or `Ctrl+F5` in Visual Studio)
3. The application will now **automatically recreate the database** with the correct schema
4. Try creating an employee again - it should work! ?

### Option 2: Manual Database Fix (If Option 1 doesn't work)
1. Open your **XAMPP Control Panel**
2. Click **Admin** next to MySQL to open phpMyAdmin
3. Select your **`hrms_database`** database
4. Click **SQL** tab and run this script:

```sql
-- Fix Employee table schema
ALTER TABLE Employees
ADD COLUMN IF NOT EXISTS EmployeeCode VARCHAR(20) NULL,
ADD COLUMN IF NOT EXISTS Address TEXT NULL,
ADD COLUMN IF NOT EXISTS DateOfBirth DATE NULL,
ADD COLUMN IF NOT EXISTS Gender VARCHAR(10) NULL,
ADD COLUMN IF NOT EXISTS Nationality VARCHAR(50) NULL DEFAULT 'Indian',
ADD COLUMN IF NOT EXISTS EmergencyContactName VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS EmergencyContactPhone VARCHAR(20) NULL,
ADD COLUMN IF NOT EXISTS IsRegistrationComplete BOOLEAN NOT NULL DEFAULT FALSE,
ADD COLUMN IF NOT EXISTS RegistrationToken VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS RegistrationTokenExpiry DATETIME NULL,
ADD COLUMN IF NOT EXISTS ProfileImagePath VARCHAR(255) NULL,
ADD COLUMN IF NOT EXISTS ModifiedDate DATETIME NULL;

-- Update existing employees with employee codes
UPDATE Employees 
SET EmployeeCode = CONCAT('OI', 
    UPPER(LEFT(FirstName, 2)), 
    UPPER(LEFT(LastName, 2)), 
    YEAR(HireDate), 
    LPAD(EmployeeId, 4, '0')
) 
WHERE EmployeeCode IS NULL OR EmployeeCode = '';

-- Set registration status for existing employees
UPDATE Employees 
SET IsRegistrationComplete = TRUE 
WHERE IsRegistrationComplete IS NULL;
```

## Test the Fix
1. Login with admin credentials: **admin@hrms.com** / **admin123**
2. Go to **Employee Management**
3. Click **"Add New Employee"**
4. Fill in the employee details:
   - First Name: **Test**
   - Last Name: **Employee**
   - Email: **test@example.com**
   - Department: **IT**
   - Position: **Developer**
   - Salary: **500000**
5. Click **"Create Employee & Send Email"**
6. You should see: **"Employee created successfully!"** ?

## What the Fix Does
- ? Adds all missing columns to the `Employees` table
- ? Generates employee codes for existing employees
- ? Sets up registration system properly
- ? Maintains all existing employee data

## Expected Result
- **Employee Code**: Auto-generates like `OITEEM20250001`
- **Email Notification**: Sends welcome email (if SMTP is working)
- **No More Errors**: Employee creation works perfectly! ??

---
## If You Still Have Issues:
1. Check that **MySQL is running** in XAMPP
2. Verify **database connection** in the application logs
3. Make sure the **database name is correct**: `hrms_database`

The application is now ready to work with the complete Employee Management System! ??