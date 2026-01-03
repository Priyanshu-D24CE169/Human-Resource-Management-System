# Salary Feature Fix Guide

## Issue: Salary field not showing in database / No option for editing

### Problem
The Salary table might not be created in the database, or the relationship is not properly set up.

### Solution Steps

#### Step 1: Check Current Database State
Run this SQL query in SQL Server Management Studio or via command:

```sql
-- Check if Salaries table exists
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'Salaries'

-- If it exists, check its structure
SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Salaries'

-- Check if any salary records exist
SELECT COUNT(*) FROM Salaries
```

#### Step 2: Recreate Database Migration (if table is missing)

**Option A: If you want to keep existing data**

1. Open Package Manager Console or Terminal
2. Add a new migration:
```bash
dotnet ef migrations add AddSalaryTable
```

3. Update the database:
```bash
dotnet ef database update
```

**Option B: If you can recreate the database (WARNING: This deletes all data)**

1. Drop the existing database:
```bash
dotnet ef database drop -f
```

2. Remove all migrations:
```bash
# Delete the Migrations folder manually
```

3. Create fresh migration:
```bash
dotnet ef migrations add InitialCreate
```

4. Create database:
```bash
dotnet ef database update
```

#### Step 3: Verify Salary Table Structure

The Salaries table should have these columns:
- SalaryId (PK)
- EmployeeId (FK)
- MonthlyWage
- YearlyWage
- WorkingDaysPerMonth
- PerDayWage
- Basic
- HRA
- PF
- Allowances
- Deductions
- ProfessionalTax
- EffectiveFrom
- CreatedDate
- UpdatedDate

#### Step 4: Manually Create Salary Table (if migration fails)

Run this SQL script:

```sql
-- Create Salaries table
CREATE TABLE Salaries (
    SalaryId INT PRIMARY KEY IDENTITY(1,1),
    EmployeeId INT NOT NULL,
    MonthlyWage DECIMAL(18,2) NOT NULL,
    YearlyWage DECIMAL(18,2) NOT NULL,
    WorkingDaysPerMonth INT NOT NULL DEFAULT 26,
    PerDayWage DECIMAL(18,2) NOT NULL,
    Basic DECIMAL(18,2) NOT NULL,
    HRA DECIMAL(18,2) NOT NULL,
    PF DECIMAL(18,2) NOT NULL,
    Allowances DECIMAL(18,2) NOT NULL,
    Deductions DECIMAL(18,2) NOT NULL,
    ProfessionalTax DECIMAL(18,2) NOT NULL,
    EffectiveFrom DATETIME2 NOT NULL,
    CreatedDate DATETIME2 NOT NULL,
    UpdatedDate DATETIME2 NULL,
    CONSTRAINT FK_Salaries_Employees FOREIGN KEY (EmployeeId) 
        REFERENCES Employees(EmployeeId) ON DELETE CASCADE
);

-- Create index for faster queries
CREATE INDEX IX_Salaries_EmployeeId ON Salaries(EmployeeId);
```

#### Step 5: Add Salary for Existing Employees

If you have employees without salary records, run:

```sql
-- Check employees without salary
SELECT e.EmployeeId, e.FirstName, e.LastName, e.LoginId
FROM Employees e
LEFT JOIN Salaries s ON e.EmployeeId = s.EmployeeId
WHERE s.SalaryId IS NULL;

-- Add default salary for employees without one (adjust amount as needed)
INSERT INTO Salaries (
    EmployeeId, 
    MonthlyWage, 
    YearlyWage, 
    WorkingDaysPerMonth, 
    PerDayWage, 
    Basic, 
    HRA, 
    PF, 
    Allowances, 
    Deductions, 
    ProfessionalTax, 
    EffectiveFrom, 
    CreatedDate
)
SELECT 
    e.EmployeeId,
    30000.00 as MonthlyWage,
    360000.00 as YearlyWage,
    26 as WorkingDaysPerMonth,
    1153.85 as PerDayWage,
    15000.00 as Basic,
    6000.00 as HRA,
    3600.00 as PF,
    3000.00 as Allowances,
    0.00 as Deductions,
    200.00 as ProfessionalTax,
    GETDATE() as EffectiveFrom,
    GETDATE() as CreatedDate
FROM Employees e
LEFT JOIN Salaries s ON e.EmployeeId = s.EmployeeId
WHERE s.SalaryId IS NULL 
  AND e.EmployeeId != (SELECT EmployeeId FROM AspNetUsers WHERE UserName = 'ADMIN001');
```

#### Step 6: Verify in Application

1. **Restart the application**
2. **Login as Admin**
3. **Go to any Employee Profile**
4. **Check if "Salary Info" tab appears**
5. **Click on "Salary Info" tab**
6. **You should see salary fields**
7. **Try editing and updating salary**

### Troubleshooting

#### Issue: Salary tab not showing
**Cause:** User is not an Admin or `CanEditSalary` is false

**Solution:**
- Ensure you're logged in as Admin (ADMIN001)
- Check that the user has Admin role in database

#### Issue: "No salary information available" message
**Cause:** No salary record exists for the employee

**Solution 1: Via Application (Recommended)**
- Create the employee again through the UI
- The Create Employee process automatically creates a salary record

**Solution 2: Via SQL**
- Use the SQL script in Step 5 to add salary records

#### Issue: Cannot update salary
**Cause:** Form submission error or validation issue

**Solution:**
1. Check browser console for errors (F12)
2. Check application logs
3. Ensure all required fields have values
4. Try entering different values

### Quick Fix Commands

```bash
# 1. Check if EF tools are installed
dotnet tool list -g

# 2. Install EF tools if missing
dotnet tool install --global dotnet-ef

# 3. Check current migrations
dotnet ef migrations list

# 4. Add new migration for salary
dotnet ef migrations add AddSalaryFeature

# 5. Update database
dotnet ef database update

# 6. If all else fails, reset database
dotnet ef database drop -f
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Verification Checklist

- [ ] Salaries table exists in database
- [ ] Salaries table has correct columns
- [ ] Foreign key relationship to Employees exists
- [ ] Salary records exist for employees
- [ ] Admin user can see "Salary Info" tab
- [ ] Salary form displays correctly
- [ ] Can update salary values
- [ ] Updated values are saved in database

### Still Not Working?

1. **Check logs** - Look at console output for errors
2. **Check database** - Verify Salaries table and data
3. **Check migration files** - Ensure migration includes Salaries table
4. **Check ApplicationDbContext** - Ensure DbSet<Salary> is present
5. **Restart application** - Sometimes changes need a fresh start

### Support

If the issue persists:
1. Check the error message in the application
2. Check the database schema
3. Review the migration files
4. Check application logs
5. Verify admin role assignment
