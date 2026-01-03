# Complete Fix Guide - Email & Salary Issues

## Issues Fixed

### 1. ? Email Configuration Fixed
### 2. ? Salary Feature Enhanced

---

## Issue 1: Email Settings Configuration

### Problem
- appsettings.json had `SmtpSettings` but code expects `EmailSettings`
- App password had spaces which could cause authentication issues

### Solution Applied

**Changed in `appsettings.json`:**
```json
// OLD (WRONG):
"SmtpSettings": {
  "Password": "neok kefv dzzr zlzz"  // with spaces
}

// NEW (CORRECT):
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": "587",
  "SenderEmail": "samarthbhalala@gmail.com",
  "SenderName": "Hr management system",
  "Username": "samarthbhalala@gmail.com",
  "Password": "neokkefvdzzrzlzz"  // no spaces
}
```

### ? Email Now Working
- Configuration matches what EmailService expects
- Password format is correct
- All email features should work now

---

## Issue 2: Salary Information Not Showing

### Problem
The salary table might not exist in your database or employees don't have salary records.

### ?? Diagnosis Steps

Run this PowerShell script to diagnose:
```powershell
.\fix-salary.ps1
```

This will:
1. Check if database is accessible
2. Check if Salaries table exists
3. Check if salary records exist
4. Provide recommendations
5. Offer to run migrations if needed

### ?? Solutions

#### Solution 1: Quick Fix via PowerShell (Recommended)

```powershell
# Run the diagnostic and fix script
.\fix-salary.ps1

# Follow the prompts to:
# 1. Create migration if table is missing
# 2. Update database
```

#### Solution 2: Manual Migration

If Salaries table doesn't exist:

```bash
# Create migration
dotnet ef migrations add AddSalaryFeature

# Update database
dotnet ef database update
```

#### Solution 3: Add Salary for Existing Employees

If table exists but employees don't have salary records:

**Option A: Via SQL Script**
1. Open SQL Server Management Studio or Azure Data Studio
2. Connect to: `(localdb)\mssqllocaldb`
3. Select database: `HRMS_DB`
4. Run the script: `CreateSalariesForEmployees.sql`

**Option B: Via Application (Easier)**
1. Login as Admin
2. Go to Employee ? Profile (for any employee)
3. Click "Salary Info" tab
4. You'll see a form to create salary information
5. Fill in the details (defaults provided)
6. Click "Create Salary Information"

#### Solution 4: Fresh Database (Last Resort)

?? **WARNING:** This deletes all data!

```bash
# Drop database
dotnet ef database drop -f

# Remove migrations
Remove-Item Migrations -Recurse -Force

# Create fresh migration
dotnet ef migrations add InitialCreate

# Create database
dotnet ef database update

# Run the application
# Login as Admin (ADMIN001 / Admin@123)
# Create employees again
```

---

## ?? How to Verify Fixes

### Verify Email Fix

1. **Restart application**
2. **Login as Admin**
3. **Create a new employee** OR **Reset password** for existing employee
4. **Check console logs** for:
```
Attempting to send email to user@example.com via smtp.gmail.com:587
Connecting to SMTP server: smtp.gmail.com:587
Authenticating with username: samarthbhalala@gmail.com
Sending email to: user@example.com
? Email sent successfully to user@example.com
```
5. **Check employee's email inbox**

### Verify Salary Feature

1. **Login as Admin** (ADMIN001 / Admin@123)
2. **Go to Employees** menu
3. **Click on any employee name** to view profile
4. **Check tabs** - You should see:
   - Resume
   - Private Info
   - **Salary Info** ? This should be visible
   - Security

5. **Click "Salary Info" tab**
6. **You should see either:**
   - **Option A:** Salary form with data (if salary exists)
   - **Option B:** "Create Salary Information" button (if no salary)

7. **If creating new salary:**
   - Fill in Monthly Wage (default: ?30,000)
   - Other fields are pre-filled with percentages
   - Click "Create Salary Information"
   - Should see success message

8. **If editing existing salary:**
   - Modify any value
   - Click "Update Salary"
   - Should see success message

---

## ?? Files Created

### 1. `SALARY_FIX_GUIDE.md`
Complete guide with:
- SQL scripts to check/create tables
- Migration commands
- Troubleshooting steps
- Verification checklist

### 2. `fix-salary.ps1`
PowerShell diagnostic tool that:
- Checks database connection
- Verifies Salaries table exists
- Counts salary records
- Provides recommendations
- Can create migrations automatically

### 3. `CreateSalariesForEmployees.sql`
SQL script that:
- Finds employees without salary
- Creates default salary records
- Shows summary of all salaries

### 4. Enhanced `Views/Employee/Profile.cshtml`
Now includes:
- Form to create salary if it doesn't exist
- Default values pre-filled
- Clear instructions
- Better UI/UX

---

## ?? Quick Start - Apply All Fixes

### Method 1: Using PowerShell (Easiest)

```powershell
# 1. Run the salary fix script
.\fix-salary.ps1

# 2. Follow the prompts

# 3. Restart your application

# 4. Test both features
```

### Method 2: Manual Steps

```bash
# 1. Check if database needs migration
dotnet ef migrations list

# 2. If Salaries is missing, create migration
dotnet ef migrations add AddSalaryFeature

# 3. Update database
dotnet ef database update

# 4. Restart application
dotnet run

# 5. Login and test
```

---

## ? Success Indicators

### Email Working:
- ? No errors in console when creating employee
- ? Success message: "Credentials have been sent to..."
- ? Email received in inbox
- ? Email contains Login ID and password

### Salary Working:
- ? "Salary Info" tab visible in employee profile
- ? Can view salary information
- ? Can create salary if missing
- ? Can update existing salary
- ? Changes are saved in database
- ? Calculated values (Yearly, Per Day) are correct

---

## ?? Still Having Issues?

### Issue: Email still not sending

**Check:**
1. Gmail App Password is correct (no spaces)
2. 2-Factor Authentication is enabled on Gmail
3. Check application logs for detailed errors
4. Try sending test email from Gmail to verify account

**Test:**
- Try resetting password for an employee
- Check if credentials appear in warning message
- If they do, email sending is the issue
- If not, password reset logic has issues

### Issue: Salary tab not showing

**Check:**
1. Are you logged in as Admin?
   - Only Admin can see "Salary Info" tab
   - Login ID: ADMIN001
   - Password: Admin@123

2. Does Salaries table exist?
   ```sql
   SELECT * FROM INFORMATION_SCHEMA.TABLES 
   WHERE TABLE_NAME = 'Salaries'
   ```

3. Run the diagnostic script:
   ```powershell
   .\fix-salary.ps1
   ```

### Issue: Cannot create/update salary

**Check:**
1. Browser console for JavaScript errors (F12)
2. Application logs for server errors
3. Database connection is working
4. Form validation is passing

**Try:**
- Clear browser cache
- Try different browser
- Check Network tab in browser dev tools
- Look for 400/500 errors

---

## ?? Support Resources

### Documentation Files:
- `README.md` - Project overview
- `DATABASE_SETUP.md` - Database setup guide
- `EMAIL_TROUBLESHOOTING.md` - Email setup guide
- `FEATURES_GUIDE.md` - Feature usage guide
- `SALARY_FIX_GUIDE.md` - Salary specific fixes
- This file - Complete fix guide

### Scripts:
- `setup.ps1` - Initial setup
- `fix-salary.ps1` - Salary diagnostic tool
- `CreateSalariesForEmployees.sql` - Add salary records

---

## ?? Summary

### What Was Fixed:

1. **Email Configuration**
   - ? Changed SmtpSettings to EmailSettings
   - ? Removed spaces from app password
   - ? Email service now properly configured

2. **Salary Feature**
   - ? Added diagnostic script (fix-salary.ps1)
   - ? Added SQL script to create salary records
   - ? Enhanced UI to create salary from profile page
   - ? Added comprehensive troubleshooting guide

### Next Steps:

1. **Restart your application**
2. **Run the diagnostic script**: `.\fix-salary.ps1`
3. **Follow recommendations** from the script
4. **Test email** by creating/resetting employee
5. **Test salary** by viewing employee profile
6. **Check this guide** if issues persist

---

**Everything should now be working! ??**

If you still encounter issues, check the specific troubleshooting guides or the diagnostic script output.
