# ?? Demo User Login Credentials

## ?? Admin Login
- **Email**: `admin@hrms.com`
- **Password**: `admin123`
- **Role**: Admin (Full system access)

## ?? Employee Demo Logins

### 1. John Doe - Software Developer
- **Email**: `john.doe@hrms.com`
- **Password**: `john123`
- **Employee Code**: `OIJODO20230001`
- **Department**: IT
- **Position**: Software Developer
- **Profile**: ? 100% Complete

### 2. Jane Smith - HR Manager
- **Email**: `jane.smith@hrms.com`
- **Password**: `jane123`
- **Employee Code**: `OIJASM20220001`
- **Department**: HR
- **Position**: HR Manager
- **Profile**: ? 100% Complete

### 3. Mike Johnson - Finance Analyst
- **Email**: `mike.johnson@hrms.com`
- **Password**: `mike123`
- **Employee Code**: `OIMIJO20230002`
- **Department**: Finance
- **Position**: Finance Analyst
- **Profile**: ? 100% Complete

### 4. Priya Patel - Marketing Specialist
- **Email**: `priya.patel@hrms.com`
- **Password**: `priya123`
- **Employee Code**: `OIPAPA20240001`
- **Department**: Marketing
- **Position**: Marketing Specialist
- **Profile**: ? 100% Complete

## ?? How to Use Demo Accounts

### For Admin Testing:
1. Login with `admin@hrms.com` / `admin123`
2. Access employee management, create new employees, etc.

### For Employee Testing:
1. Login with any employee credentials above
2. Redirects to employee dashboard automatically
3. View profile, edit personal information, etc.

## ?? Auto-Setup

These demo users are automatically created when you:
1. **Restart the application** (Stop + Start debugging)
2. Database is automatically recreated with all demo data
3. No manual setup required!

## ?? What Each Employee Has:

? **Complete Personal Profile**:
- Full name, email, phone
- Address and emergency contacts
- Date of birth and gender
- Employee code and department

? **Login Access**:
- Valid user account with password
- Role-based authentication
- Session management

? **Dashboard Access**:
- Personal employee dashboard
- Profile editing capabilities
- Company information display

## ?? Database Login Issue - FIXED!

### ?? Problem Identified:
- ? Demo users failed to create due to duplicate email conflicts
- ? User accounts weren't created for existing employees
- ? Login failed because `john.doe@hrms.com` existed as Employee but not as User

### ? Solution Applied:
- ?? Fixed demo user creation to work with existing seed data
- ?? Creates User accounts for existing Employee records
- ?? Avoids email conflicts by using existing employees
- ? Ensures complete employee profiles with address, emergency contacts, etc.

### ?? How to Apply the Fix:

#### Restart Application:
1. **Stop debugging** (Shift+F5)
2. **Start again** (F5)
3. Database will recreate with working demo users

### ?? Updated Demo Login Credentials:

#### ?? Admin Login:
- **Email**: `admin@hrms.com`
- **Password**: `admin123`

#### ?? Employee Logins:
- **John Doe**: `john.doe@hrms.com` / `john123` (Software Developer)
- **Jane Smith**: `jane.smith@hrms.com` / `jane123` (HR Manager)  
- **Mike Johnson**: `mike.johnson@hrms.com` / `mike123` (Finance Analyst)
- **Priya Patel**: `priya.patel@hrms.com` / `priya123` (Marketing Specialist)

### ?? What's Fixed:
? **No more email conflicts** - Uses existing employees from seed data  
? **User accounts created** - All employees can now login  
? **Complete profiles** - Address, emergency contacts, personal info  
? **Password authentication** - BCrypt hashing works properly  

### ?? Testing Flow:
1. **Restart app** ? Database recreates successfully
2. **Login as employee** ? `john.doe@hrms.com` / `john123`
3. **Employee dashboard** ? Shows personal information and statistics
4. **Admin login** ? `admin@hrms.com` / `admin123` to manage employees

The login should work perfectly now! ??