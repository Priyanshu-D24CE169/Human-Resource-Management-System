-- SQL Script to fix Employee table schema
-- This script adds the missing columns to match the updated Employee model

USE hrms_database;

-- First, let's check the current structure of the Employees table
DESCRIBE Employees;

-- Add missing columns to the Employees table
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

-- Update existing employees to have employee codes if they don't have them
UPDATE Employees 
SET EmployeeCode = CONCAT('OI', 
    UPPER(LEFT(FirstName, 2)), 
    UPPER(LEFT(LastName, 2)), 
    YEAR(HireDate), 
    LPAD(EmployeeId, 4, '0')
) 
WHERE EmployeeCode IS NULL OR EmployeeCode = '';

-- Set IsRegistrationComplete to TRUE for existing employees
UPDATE Employees 
SET IsRegistrationComplete = TRUE 
WHERE IsRegistrationComplete IS NULL OR IsRegistrationComplete = FALSE;

-- Check the updated structure
DESCRIBE Employees;

-- Verify the data
SELECT EmployeeId, EmployeeCode, FirstName, LastName, HireDate, IsRegistrationComplete 
FROM Employees 
ORDER BY EmployeeId;