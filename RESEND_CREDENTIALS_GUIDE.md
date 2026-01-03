# Resend Credentials Feature - User Guide

## Overview

The **Resend Credentials** feature allows administrators to help employees who have lost or forgotten their login information. This feature includes two main actions:

1. **Resend Login ID** - Sends a reminder email with the employee's Login ID
2. **Reset Password** - Generates a new password and sends complete credentials via email

---

## ?? Features Added

### 1. Resend Login ID
- Sends email reminder with employee's Login ID
- Does NOT reset the password
- Useful when employee forgets their Login ID
- Quick and safe option

### 2. Reset Password & Send Credentials
- Generates a new random password (meets all security requirements)
- Invalidates the old password
- Sends email with Login ID and new password
- Forces employee to change password on next login
- Useful when employee forgets their password

---

## ?? Where to Access

### Option 1: Employee List Page
1. Login as **Admin**
2. Navigate to **Employees** menu
3. In the employee table, click the **three-dot menu (?)** button
4. Select either:
   - **Resend Login ID** - Send login reminder
   - **Reset Password** - Reset password and send new credentials

### Option 2: Employee Profile Page
1. Login as **Admin**
2. Open any employee profile
3. Click **Admin Actions** dropdown (top-right)
4. Select either:
   - **Resend Login ID**
   - **Reset Password & Send**

---

## ?? How It Works

### Scenario 1: Employee Forgot Login ID

**Admin Action:**
1. Click **Resend Login ID** for the employee
2. Confirm the action in the popup
3. System sends email with Login ID reminder

**Email Sent:**
```
Subject: HRMS - Login Credentials Reminder

Dear [Employee Name],

As requested, here are your login credentials for the HRMS system.

Login Credentials:
- Login ID: OIJD20230001
- Password: (Your current password - if forgotten, request password reset)

Note: If you've forgotten your password, please contact the HR 
administrator to request a password reset.

Login URL: HRMS Portal
```

**Result:**
? Employee receives Login ID
? Password remains unchanged
? Employee can login with existing password

---

### Scenario 2: Employee Forgot Password

**Admin Action:**
1. Click **Reset Password** for the employee
2. Confirm the action in the popup (warns that current password will be invalidated)
3. System generates new random password
4. System sends email with new credentials

**Email Sent:**
```
Subject: HRMS - Password Reset & Credentials

Dear [Employee Name],

Your password has been reset by the administrator.

New Login Credentials:
- Login ID: OIJD20230001
- New Temporary Password: Abc@123Xyz!

Important: Please login and change your password immediately 
for security reasons.

Login URL: HRMS Portal
```

**Result:**
? Old password is invalidated
? New secure password is generated
? Employee receives complete credentials
? Employee MUST change password on first login

---

## ? Success Messages

### Resend Login ID Success:
```
? Login ID has been sent to employee@example.com. 
  For password reset, use the 'Reset Password' option.
```

### Reset Password Success:
```
? Password reset successfully! 
  New credentials have been sent to employee@example.com
```

### Email Failure (Fallback):
```
? Password reset successfully, but email could not be sent to employee@example.com. 
  Please provide new credentials manually: 
  Login ID: OIJD20230001, Password: Abc@123Xyz!
```

---

## ?? Security Features

### Password Reset Security:
1. **New password is randomly generated** with:
   - Minimum 12 characters
   - Uppercase letters
   - Lowercase letters
   - Digits
   - Special characters (@#$!%^&*)

2. **Forces password change**:
   - `MustChangePassword` flag is set to `true`
   - Employee redirected to Change Password page on login

3. **Invalidates old password**:
   - Old password cannot be used anymore
   - Uses secure password reset token

4. **Confirmation required**:
   - Admin must confirm action
   - Warning shown that old password will be invalidated

### Audit Trail:
- All actions are logged
- Email send status is tracked
- Errors are logged with details

---

## ?? Use Cases

### Use Case 1: New Employee Never Received Welcome Email
**Solution:** Use **Reset Password** to generate fresh credentials and resend

### Use Case 2: Employee Lost Welcome Email
**Solution:** Use **Reset Password** to send new credentials

### Use Case 3: Employee Forgot Login ID Only
**Solution:** Use **Resend Login ID** (faster, doesn't reset password)

### Use Case 4: Employee Forgot Password
**Solution:** Use **Reset Password** to generate new password

### Use Case 5: Security Breach - Force Password Change
**Solution:** Use **Reset Password** to immediately invalidate old password

### Use Case 6: Employee Changed Email
**Solution:** 
1. Update employee email in profile
2. Use **Reset Password** to send credentials to new email

---

## ?? Admin Workflow

### Quick Reference:

```
Employee Issue              | Admin Action           | Result
---------------------------|------------------------|---------------------------
Forgot Login ID only       | Resend Login ID        | Email with Login ID sent
Forgot Password            | Reset Password         | New password generated & sent
Lost welcome email         | Reset Password         | Fresh credentials sent
Security concern           | Reset Password         | Old password invalidated
Never received email       | Reset Password         | New credentials sent
```

---

## ?? Important Notes

### For Admin:
1. **Always confirm employee email** before resending credentials
2. **Verify employee identity** before resetting password
3. **Check spam folder** if employee doesn't receive email
4. **Keep track** of password resets for security audit
5. **If email fails**, credentials are shown in warning message - provide them manually

### For Employees:
1. **Check spam/junk folder** for credential emails
2. **Change password immediately** after receiving reset email
3. **Don't share** credentials with anyone
4. **Contact HR** if you don't receive email within 5 minutes

---

## ?? Troubleshooting

### Issue 1: Email Not Received
**Possible Causes:**
- Email in spam/junk folder
- Wrong email address in employee profile
- Email service configuration issue

**Solution:**
1. Check spam folder
2. Verify email address in employee profile
3. Check warning message for credentials (if email failed)
4. Manually provide credentials to employee
5. Check email service logs

### Issue 2: Reset Password Not Working
**Possible Causes:**
- Employee account not found
- User account not linked to employee

**Solution:**
- Check error message
- Verify employee has a user account
- Check database for data consistency

### Issue 3: Employee Can't Login After Reset
**Possible Causes:**
- Using old password instead of new one
- Password was reset again
- Copy-paste error (extra spaces)

**Solution:**
- Confirm employee is using NEW password from email
- Check if password was reset multiple times
- Type password manually instead of copy-paste
- Reset password again if needed

---

## ?? Email Logs

Check application logs for email status:

### Success Log:
```
Attempting to send email to employee@example.com via smtp.gmail.com:587
Connecting to SMTP server: smtp.gmail.com:587
Authenticating with username: admin@hrms.com
Sending email to: employee@example.com
? Email sent successfully to employee@example.com
```

### Error Log:
```
? Error sending email to employee@example.com: [error message]
Stack Trace: [details]
```

---

## ?? UI Elements

### Employee List Page:
- **Three-dot menu (?)** button in Actions column
- Dropdown with:
  - "Resend Login ID" option
  - "Reset Password" option (in red color to indicate caution)

### Employee Profile Page:
- **Admin Actions** dropdown button (yellow/warning color)
- Same two options as above
- Only visible to Admin users

### Confirmations:
- **Resend Login ID**: Simple confirmation
- **Reset Password**: Warning that old password will be invalidated

---

## ?? Comparison: Resend vs Reset

| Feature | Resend Login ID | Reset Password |
|---------|----------------|----------------|
| Sends Login ID | ? Yes | ? Yes |
| Sends Password | ? No (reminder only) | ? Yes (new password) |
| Invalidates old password | ? No | ? Yes |
| Generates new password | ? No | ? Yes |
| Forces password change | ? No | ? Yes |
| Use when | Forgot Login ID | Forgot Password |
| Risk level | ?? Low | ?? Medium (invalidates old) |

---

## ? Best Practices

### For Administrators:
1. ? Use **Resend Login ID** when possible (less disruptive)
2. ? Use **Reset Password** only when necessary
3. ? Verify employee identity before reset
4. ? Keep note of resets for security audit
5. ? Inform employee before resetting password
6. ? Check if email was delivered successfully

### For Security:
1. ?? Monitor frequent password reset requests
2. ?? Investigate if same employee requests multiple resets
3. ?? Ensure emails are sent only to verified addresses
4. ?? Log all credential management actions
5. ?? Review logs periodically for unusual activity

---

## ?? Support

If you encounter issues:
1. Check application logs
2. Verify email configuration (EMAIL_TROUBLESHOOTING.md)
3. Check employee email address is correct
4. Look for warning messages in UI
5. Contact system administrator

---

**Last Updated:** 2024  
**Version:** 1.0  
**Feature:** Resend Credentials & Password Reset
