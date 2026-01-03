-- HRMS - Create Salary Records for Existing Employees
-- Run this script if employees don't have salary information

-- Step 1: Check employees without salary
PRINT 'Checking employees without salary records...'
SELECT 
    e.EmployeeId, 
    e.FirstName, 
    e.LastName, 
    e.LoginId,
    e.Email,
    e.Designation
FROM Employees e
LEFT JOIN Salaries s ON e.EmployeeId = s.EmployeeId
WHERE s.SalaryId IS NULL
  AND e.IsActive = 1;

PRINT ''
PRINT 'Adding default salary records for employees without salary...'

-- Step 2: Insert default salary for employees without one
-- Default values:
-- Monthly: 30,000
-- Yearly: 360,000
-- Working Days: 26
-- Per Day: 1,153.85
-- Basic (50%): 15,000
-- HRA (20%): 6,000
-- PF (12%): 3,600
-- Allowances (10%): 3,000
-- Professional Tax: 200

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
    CAST(30000.00 / 26 AS DECIMAL(18,2)) as PerDayWage,
    15000.00 as Basic,        -- 50% of Monthly
    6000.00 as HRA,           -- 20% of Monthly
    3600.00 as PF,            -- 12% of Monthly
    3000.00 as Allowances,    -- 10% of Monthly
    0.00 as Deductions,
    200.00 as ProfessionalTax,
    e.JoiningDate as EffectiveFrom,
    GETDATE() as CreatedDate
FROM Employees e
LEFT JOIN Salaries s ON e.EmployeeId = s.EmployeeId
WHERE s.SalaryId IS NULL 
  AND e.IsActive = 1;

-- Step 3: Verify the results
PRINT ''
PRINT 'Salary records created successfully!'
PRINT ''
PRINT 'Summary of all employees with salary:'
SELECT 
    e.EmployeeId,
    e.LoginId,
    e.FirstName + ' ' + e.LastName as FullName,
    e.Designation,
    s.MonthlyWage,
    s.YearlyWage,
    s.EffectiveFrom
FROM Employees e
INNER JOIN Salaries s ON e.EmployeeId = s.EmployeeId
WHERE e.IsActive = 1
ORDER BY e.FirstName;

PRINT ''
PRINT 'Done! You can now view and edit salary information in the application.'
