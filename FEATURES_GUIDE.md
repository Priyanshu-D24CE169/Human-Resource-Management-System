# HRMS - Features & Usage Guide

## Table of Contents
1. [User Roles](#user-roles)
2. [Authentication Flow](#authentication-flow)
3. [Dashboard](#dashboard)
4. [Employee Management](#employee-management)
5. [Attendance System](#attendance-system)
6. [Leave Management](#leave-management)
7. [Salary Management](#salary-management)
8. [Profile Management](#profile-management)

---

## User Roles

### Admin Role
- Full access to all features
- Can create, edit, and manage employees
- View and manage salary information
- Approve/reject leave requests
- View attendance reports for all employees
- Access to all system modules

### Employee Role
- Limited access based on own data
- View and edit own profile (limited fields)
- Mark daily attendance
- Apply for leave
- View own attendance history
- Cannot access other employees' data
- Cannot view salary information of others

---

## Authentication Flow

### Login Process
1. Navigate to login page
2. Enter **Login ID** (NOT email)
3. Enter password
4. Click "Login"

### First Login Experience
1. Login with temporary credentials (received via email)
2. System detects first login
3. Redirected to **Change Password** page
4. Must set a new secure password
5. After password change, access granted to dashboard

### Password Requirements
- Minimum 6 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit
- At least one special character (@, #, $, etc.)

---

## Dashboard

### Overview
The dashboard provides a quick snapshot of:
- Total number of employees
- Present employees today
- Employees on leave
- Absent employees

### Employee Cards
- Display employee profile photo
- Show basic information (Name, Designation, Department)
- Status indicator:
  - ?? **Green Badge** = Present today
  - ?? **Blue Badge** = On leave today
  - ?? **Yellow Badge** = Absent today
- Click on any card to view full employee profile

### Admin vs Employee Dashboard
- **Admin:** Sees all employees
- **Employee:** Sees only their own card

---

## Employee Management

### Creating a New Employee (Admin Only)

#### Step 1: Navigate to Employees
- Click "Employees" in navigation menu
- Click "Add New Employee" button

#### Step 2: Fill Employee Information

**Personal Information:**
- First Name (Required)
- Last Name (Required)
- Email (Required) - Used for communications
- Phone Number
- Date of Birth (Required)
- Gender

**Address Information:**
- Full Address
- City
- State
- Zip Code

**Employment Details:**
- Department
- Designation
- Joining Date (Required) - Used for Login ID generation

**Bank & Documents:**
- Bank Name
- Account Number
- IFSC Code
- PAN Number
- Aadhar Number

**Emergency Contact:**
- Emergency Contact Name
- Emergency Contact Phone

**Salary Information:**
- Monthly Salary (Required) - Auto-calculates components

**Upload Files:**
- Profile Photo (JPG, PNG)
- Resume (PDF, DOC, DOCX)

#### Step 3: System Actions
1. Validates all required fields
2. Generates unique Login ID using format:
   - `[CompanyCode][FirstInitial][LastInitial][Year][Serial]`
   - Example: `OIJD20230001`
3. Generates random secure password
4. Creates user account
5. Calculates salary components:
   - Basic: 50% of monthly salary
   - HRA: 20% of monthly salary
   - PF: 12% of monthly salary
   - Allowances: 10% of monthly salary
   - Professional Tax: ?200
6. Sends email with credentials to employee
7. Redirects to employee list

### Viewing Employee Profile
- Click on employee card from dashboard
- Or click on employee name in employee list
- View profile organized in tabs:
  - **Resume:** Basic information, contact details
  - **Private Info:** Bank details, emergency contact
  - **Salary Info:** (Admin only) Complete salary breakdown
  - **Security:** Change password option

### Editing Employee Information
- Admin can edit all fields
- Employee can edit limited fields (contact info, address)
- Click "Edit Profile" button
- Make changes
- Click "Update" to save

---

## Attendance System

### For Employees

#### Daily Check-In
1. Navigate to "Attendance" menu
2. View current date and time
3. Click "Check In" button
4. System records:
   - Date
   - Check-in time
   - Status set to "Present"
5. Confirmation message displayed

#### Daily Check-Out
1. Navigate to "Attendance" menu
2. Click "Check Out" button
3. System automatically calculates:
   - Work hours (difference between check-in and check-out)
   - Extra hours (if work hours > 8)
4. Updates attendance record

#### View Attendance History
- Table shows last 30 days of attendance
- Displays:
  - Date
  - Check-in time
  - Check-out time
  - Work hours
  - Extra hours
  - Status
  - Remarks

### For Admin

#### View All Attendance
- Navigate to "Attendance" menu
- View attendance for all employees
- See today's status at a glance

#### Attendance Reports
1. Navigate to "Attendance Report"
2. Select date range (From - To)
3. Optionally filter by specific employee
4. Click "Filter" button
5. View comprehensive attendance data:
   - Employee details
   - Date-wise attendance
   - Work hours summary
   - Extra hours tracking

---

## Leave Management

### Applying for Leave (Employee)

#### Step 1: Navigate to Leave Module
- Click "Leave" in navigation menu
- Click "Apply for Leave" button

#### Step 2: Fill Leave Request Form
- **From Date:** Start date of leave
- **To Date:** End date of leave
- **Leave Type:** Select one of:
  - Paid Leave
  - Sick Leave
  - Unpaid Leave
- **Reason:** Detailed explanation (Required)
- **Attachment:** Upload supporting document (Optional)
  - Medical certificate for sick leave
  - Other relevant documents

#### Step 3: Submit Request
- Click "Submit Request"
- System calculates total days
- Request status set to "Pending"
- Admin receives notification

### Managing Leave Requests (Admin)

#### View All Leave Requests
- Dashboard shows summary:
  - Pending requests count
  - Approved requests count
  - Rejected requests count
- All requests displayed with full details

#### Approve Leave Request
1. Locate pending leave request
2. Click "Approve" button
3. Optionally add remarks
4. Click "Approve" in modal
5. System actions:
   - Updates status to "Approved"
   - Marks attendance as "On Leave" for requested dates
   - Sends approval email to employee

#### Reject Leave Request
1. Locate pending leave request
2. Click "Reject" button
3. Enter reason for rejection (Required)
4. Click "Reject" in modal
5. System actions:
   - Updates status to "Rejected"
   - Sends rejection email with reason to employee

### Leave Request Status
- **Pending:** Awaiting admin decision (Yellow badge)
- **Approved:** Accepted by admin (Green badge)
- **Rejected:** Denied by admin (Red badge)

### Employee Actions
- **Cancel:** Can cancel only pending requests
- **View:** Can view all own leave requests
- Cannot edit submitted requests

---

## Salary Management (Admin Only)

### View Salary Information
- Navigate to employee profile
- Click "Salary Info" tab
- View complete salary breakdown

### Salary Components

#### Fixed Components
- **Monthly Wage:** Base monthly salary
- **Yearly Wage:** Monthly wage × 12
- **Working Days Per Month:** Default 26 days
- **Per Day Wage:** Monthly wage ÷ Working days

#### Breakdown
- **Basic:** 50% of monthly wage
- **HRA (House Rent Allowance):** 20% of monthly wage
- **PF (Provident Fund):** 12% of monthly wage (deduction)
- **Allowances:** Other allowances
- **Deductions:** Any deductions
- **Professional Tax:** Fixed amount (default ?200)

### Updating Salary
1. Navigate to employee profile
2. Click "Salary Info" tab
3. Modify salary components
4. Click "Update Salary"
5. System recalculates:
   - Yearly wage
   - Per day wage
6. Salary update saved

### Salary Calculation Logic
```
Yearly Wage = Monthly Wage × 12
Per Day Wage = Monthly Wage ÷ Working Days Per Month
Net Salary = (Basic + HRA + Allowances) - (PF + Deductions + Professional Tax)
```

### Attendance-Based Salary
- Absent days reduce payable days
- Unpaid leave reduces salary
- Paid leave does not affect salary
- Sick leave counted as paid (if approved)

---

## Profile Management

### Profile Tabs

#### 1. Resume Tab
- First Name, Last Name
- Date of Birth, Gender
- Department, Designation
- Address (Full address, City, State, Zip)
- Resume download link (if uploaded)

#### 2. Private Info Tab
- Emergency Contact Name & Phone
- Bank Name
- Account Number
- IFSC Code
- PAN Number
- Aadhar Number

#### 3. Salary Info Tab (Admin Only)
- Complete salary breakdown
- Edit capabilities
- Calculation details

#### 4. Security Tab
- Current Login ID (read-only)
- Change Password option

### Editing Profile
- **Admin:** Can edit all fields
- **Employee:** Can edit:
  - Contact information
  - Address
  - Emergency contact
  - Profile photo
- **Cannot Edit:**
  - Login ID
  - Email (used for system communications)
  - Joining date
  - Company-assigned fields

### Uploading Profile Photo
1. Navigate to "Edit Profile"
2. Click "Choose File" under Profile Photo
3. Select image (JPG, PNG)
4. Click "Update"
5. Photo displayed on:
   - Dashboard cards
   - Employee list
   - Profile page

---

## Email Notifications

### When Sent
1. **Employee Creation:** Credentials sent to new employee
2. **Leave Approval:** Confirmation sent to employee
3. **Leave Rejection:** Notification with reason sent to employee

### Email Templates

#### Employee Credentials Email
```
Subject: HRMS - Your Login Credentials

Dear [Employee Name],

Your employee account has been created successfully.

Login Credentials:
Login ID: [Auto-Generated Login ID]
Temporary Password: [Random Password]

Please login and change your password immediately.

Best Regards,
HR Team
```

#### Leave Approval Email
```
Subject: Leave Request Approved

Dear [Employee Name],

Your leave request has been approved.

Details:
- Leave Type: [Type]
- From: [Date]
- To: [Date]
- Total Days: [Days]
- Remarks: [Admin Remarks]

Best Regards,
HR Team
```

---

## Best Practices

### For Admin
1. Create employees with accurate information
2. Verify email addresses before creating accounts
3. Review leave requests promptly
4. Keep salary information updated
5. Generate regular attendance reports
6. Monitor absent employees

### For Employees
1. Change password immediately on first login
2. Mark attendance daily (Check-In and Check-Out)
3. Apply for leave in advance
4. Provide proper documentation for sick leave
5. Keep profile information updated
6. Check email for system notifications

---

## Troubleshooting

### Cannot Login
- Verify Login ID (not email)
- Check password (case-sensitive)
- Contact admin for password reset

### Cannot Check-In
- Already checked in today
- Check if already marked present

### Cannot Check-Out
- Must check in first
- Already checked out today

### Leave Request Not Approved
- Wait for admin review
- Check email for updates
- Contact HR if urgent

### Email Not Received
- Check spam folder
- Verify email address with admin
- Check email configuration

---

## Support & Contact

For technical issues or questions:
1. Contact your system administrator
2. Refer to the technical documentation
3. Check the README.md file for setup issues

---

**Last Updated:** 2024
**Version:** 1.0
