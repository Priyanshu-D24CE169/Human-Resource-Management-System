# ?? Employee Management System Testing Checklist

## ? Pre-Testing Setup

### 1. Verify Email Configuration
- [x] Gmail SMTP settings configured
- [x] App password: `ntem pcsd xqjg bpox`
- [x] Sender email: `samarthbhalala@gmail.com`

### 2. Database Configuration
- [x] MySQL server: `192.168.0.114:3306`
- [x] Database: `hrms_database`
- [x] Auto-migration enabled
- [x] Admin user seeded

### 3. Application Build
- [x] Project builds successfully
- [x] All views created
- [x] Services registered
- [x] Controllers enhanced

## ?? Testing Workflow

### Phase 1: Admin Login Test
1. **Start Application**
   ```
   dotnet run
   Navigate to: http://localhost:5194
   ```

2. **Admin Login**
   - Email: `admin@hrms.com`
   - Password: `admin123`
   - Expected: Successful login to dashboard

3. **Navigation Test**
   - Access Employee Management section
   - Verify employee list loads
   - Check existing sample employees display

### Phase 2: Employee Creation Test
1. **Create New Employee**
   - Click "Add New Employee"
   - Fill form with test data:
     ```
     First Name: Test
     Last Name: Employee
     Email: [your-real-email@gmail.com]
     Department: IT
     Position: Software Tester
     Phone: +91-9876543210
     Salary: 600000
     ```

2. **Verify Employee Code Generation**
   - Expected format: `OITEEM2025XXXX`
   - Check code is unique and follows pattern

3. **Email Notification Test**
   - Verify success message appears
   - Check logs for email sending status
   - Wait for email delivery (check inbox/spam)

### Phase 3: Employee Registration Test
1. **Check Welcome Email**
   - Open email from Odoo India HRMS
   - Verify professional template
   - Check registration link works

2. **Complete Registration**
   - Click registration link
   - Fill personal information:
     ```
     Phone: +91-9876543210
     Address: 123 Test Street, Mumbai
     DOB: 1990-01-01
     Gender: Male/Female
     Emergency Contact: John Doe
     Emergency Phone: +91-9876543211
     ```
   - Set password (min 6 characters)
   - Submit form

3. **Verify Account Creation**
   - Check success message
   - Verify redirect to login page

### Phase 4: Employee Login Test
1. **Employee Login**
   - Email: [the email used in creation]
   - Password: [password set during registration]
   - Expected: Successful login

2. **Dashboard Access**
   - Verify employee can access their dashboard
   - Check profile information

### Phase 5: Admin Management Test
1. **View Employee Details**
   - Return to admin account
   - Navigate to Employee Management
   - Click "View Details" on created employee
   - Verify all information displays correctly

2. **Registration Status**
   - Check registration shows as "Complete"
   - Verify all personal information appears

3. **Test Actions**
   - Edit employee information
   - Test resend registration (for pending employees)
   - Test employee deactivation

## ?? Expected Results

### ? Success Criteria
- [x] Admin login works with correct credentials
- [x] Employee creation generates proper Employee Code
- [x] Welcome email sends successfully
- [x] Email contains working registration link
- [x] Employee can complete registration
- [x] User account created automatically
- [x] Employee can login after registration
- [x] Employee details view shows complete information
- [x] All CRUD operations work properly

### ?? Common Issues & Solutions

#### Email Not Sending
```
Check: Application logs for SMTP errors
Fix: Verify Gmail settings, check network connectivity
```

#### Employee Code Not Generated
```
Check: Database has proper Employee table structure
Fix: Run database migration, check Employee.GenerateEmployeeCode method
```

#### Registration Link Not Working
```
Check: Token expiry, proper URL format
Fix: Regenerate token, check base URL configuration
```

#### Database Connection Issues
```
Check: MySQL server running, network connectivity
Fix: Verify connection string, check firewall settings
```

## ?? Test Data Templates

### Test Employee 1
```json
{
  "FirstName": "Alice",
  "LastName": "Johnson",
  "Email": "alice.johnson@test.com",
  "Department": "Marketing",
  "Position": "Marketing Manager",
  "Salary": 850000,
  "Phone": "+91-9876543210"
}
```

### Test Employee 2
```json
{
  "FirstName": "Bob",
  "LastName": "Wilson",
  "Email": "bob.wilson@test.com", 
  "Department": "Finance",
  "Position": "Financial Analyst",
  "Salary": 720000,
  "Phone": "+91-9876543211"
}
```

## ?? Performance Testing

### Load Test Scenarios
1. **Create 10 employees simultaneously**
2. **Send 10 registration emails in sequence**
3. **Complete 5 registrations concurrently**
4. **Test database performance with 100+ employees**

### Monitoring Points
- Email sending latency
- Database query performance
- UI responsiveness
- Memory usage during bulk operations

## ?? Success Metrics

### Functional Metrics
- 100% email delivery rate
- < 5 second employee creation time
- < 3 second registration completion
- 0% data corruption during concurrent operations

### User Experience Metrics
- Intuitive UI navigation
- Clear error messages
- Professional email templates
- Mobile-responsive design

## ?? Debug Commands

### Check Database State
```sql
SELECT EmployeeId, EmployeeCode, FirstName, LastName, IsRegistrationComplete 
FROM Employees ORDER BY CreatedDate DESC;
```

### Check Email Logs
```bash
# Check application output for email sending logs
grep "Email" application.log
```

### Test Email Configuration
```csharp
// Add to a test controller
public async Task<IActionResult> TestEmail()
{
    var result = await _emailService.SendEmployeeWelcomeEmailAsync(
        "test@example.com", 
        "Test User", 
        "OITESU20250001", 
        "http://localhost:5194/test", 
        "temp123"
    );
    return Json(new { success = result });
}
```

---

**Next Steps After Testing:**
1. Address any issues found during testing
2. Optimize performance based on results
3. Deploy to production environment
4. Setup monitoring and logging
5. Create user documentation