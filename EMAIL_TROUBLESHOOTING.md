# Email Configuration & Troubleshooting Guide

## ? Fixed Issues

### 1. Gmail App Password Format
**Problem:** The app password in `appsettings.json` had spaces which can cause authentication failure.

**Fixed:** Removed spaces from password
```json
"Password": "ntempcsdxqjgbpox"  // No spaces
```

### 2. Enhanced Error Logging
Added detailed logging to identify SMTP connection issues:
- Connection status
- Authentication status
- Send status
- Detailed error messages

### 3. Better Error Messages
Now shows:
- Exact error message from SMTP server
- Inner exception details
- If email fails, credentials are displayed in the warning message

---

## ?? Gmail SMTP Configuration

### Current Settings (in appsettings.json)
```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": "587",
  "SenderEmail": "samarthbhalala@gmail.com",
  "SenderName": "HRMS System",
  "Username": "samarthbhalala@gmail.com",
  "Password": "ntempcsdxqjgbpox"
}
```

### How to Get Gmail App Password

1. **Enable 2-Factor Authentication**
   - Go to: https://myaccount.google.com/security
   - Enable 2-Step Verification

2. **Generate App Password**
   - Go to: https://myaccount.google.com/apppasswords
   - Select "Mail" and "Windows Computer" (or Other)
   - Click "Generate"
   - **Copy the 16-character password WITHOUT SPACES**
   - Update `appsettings.json` with this password

3. **Important:** 
   - Use the app password, NOT your regular Gmail password
   - Remove all spaces from the app password
   - The password should be 16 characters long

---

## ?? Testing Email Functionality

### Test 1: Create an Employee
1. Login as Admin
2. Go to "Employees" ? "Add New Employee"
3. Fill in the form (use a valid email address you can access)
4. Click "Create Employee"
5. Check the response:
   - ? Success: "Employee created successfully! Credentials sent to [email]"
   - ?? Warning: Shows error details and credentials
   - ? Error: Check logs for details

### Test 2: Check Application Logs
Look for these log messages:
```
Attempting to send email to [email] via smtp.gmail.com:587
Connecting to SMTP server: smtp.gmail.com:587
Authenticating with username: [username]
Sending email to: [email]
? Email sent successfully to [email]
```

Or error messages:
```
? Error sending email to [email]: [error message]
```

---

## ?? Common Issues & Solutions

### Issue 1: "Authentication failed"
**Cause:** Wrong app password or 2FA not enabled

**Solution:**
1. Verify 2-Factor Authentication is enabled
2. Generate a new App Password
3. Copy password WITHOUT spaces
4. Update `appsettings.json`
5. Restart the application

### Issue 2: "The SMTP server requires a secure connection"
**Cause:** Wrong port or security settings

**Solution:**
- Ensure port is 587 (StartTLS)
- Or use port 465 with SSL
- The code uses `SecureSocketOptions.StartTls` for port 587

### Issue 3: "Unable to connect to the remote server"
**Cause:** Firewall or network blocking SMTP

**Solution:**
1. Check firewall settings
2. Allow outbound connections to smtp.gmail.com on port 587
3. Check corporate network restrictions
4. Try from a different network

### Issue 4: "Mailbox unavailable"
**Cause:** Invalid recipient email address

**Solution:**
- Verify the email address is correct
- Check for typos
- Ensure the email address is valid and active

### Issue 5: "Daily sending quota exceeded"
**Cause:** Gmail has sending limits

**Solution:**
- Gmail allows 500 emails/day for regular accounts
- Wait 24 hours for quota reset
- Or use Google Workspace for higher limits

---

## ?? Debugging Steps

### Step 1: Check Logs
Look at the console output or application logs for detailed error messages.

### Step 2: Verify Email Settings
```bash
# Check if settings are loaded correctly
# The application will log: "Email settings are not configured properly" if something is wrong
```

### Step 3: Test SMTP Connection Manually
You can use PowerShell to test SMTP:
```powershell
$smtpServer = "smtp.gmail.com"
$smtpPort = 587
$username = "samarthbhalala@gmail.com"
$password = "ntempcsdxqjgbpox"

$smtp = New-Object System.Net.Mail.SmtpClient($smtpServer, $smtpPort)
$smtp.EnableSsl = $true
$smtp.Credentials = New-Object System.Net.NetworkCredential($username, $password)

try {
    $smtp.Send("samarthbhalala@gmail.com", "samarthbhalala@gmail.com", "Test", "Test")
    Write-Host "? Email sent successfully" -ForegroundColor Green
} catch {
    Write-Host "? Error: $($_.Exception.Message)" -ForegroundColor Red
}
```

### Step 4: Check Application Output
When creating an employee, check for:
- Success message with email confirmation
- Warning message with credentials if email fails
- Error message with details

---

## ?? Alternative SMTP Providers

If Gmail doesn't work, try these alternatives:

### Outlook/Office 365
```json
{
  "SmtpServer": "smtp-mail.outlook.com",
  "SmtpPort": "587",
  "SenderEmail": "your-email@outlook.com",
  "Username": "your-email@outlook.com",
  "Password": "your-password"
}
```

### SendGrid (Recommended for Production)
```json
{
  "SmtpServer": "smtp.sendgrid.net",
  "SmtpPort": "587",
  "SenderEmail": "your-verified-email@yourdomain.com",
  "Username": "apikey",
  "Password": "your-sendgrid-api-key"
}
```

### Mailgun
```json
{
  "SmtpServer": "smtp.mailgun.org",
  "SmtpPort": "587",
  "SenderEmail": "your-email@yourdomain.com",
  "Username": "postmaster@yourdomain.com",
  "Password": "your-mailgun-password"
}
```

---

## ?? Email Testing Checklist

- [ ] 2-Factor Authentication enabled on Gmail
- [ ] App Password generated (16 characters)
- [ ] App Password copied WITHOUT spaces
- [ ] `appsettings.json` updated with correct password
- [ ] Application restarted after config change
- [ ] Firewall allows SMTP connections
- [ ] Valid recipient email address used
- [ ] Check application logs for detailed errors
- [ ] Check email inbox (and spam folder)

---

## ?? What Happens Now

### On Success:
1. Employee is created in database
2. Login ID and password are generated
3. Email is sent to employee with:
   - Login ID
   - Temporary password
   - Link to login page
4. Success message shown to admin
5. Employee receives email and can login

### On Failure:
1. Employee is still created in database
2. Login ID and password are generated
3. Email sending fails (error logged)
4. **Warning message shows credentials to admin**
5. Admin can manually provide credentials to employee

---

## ?? Still Not Working?

### Check These:
1. **Console/Logs** - Look for detailed error messages
2. **Email Settings** - Verify all settings are correct
3. **Network** - Check firewall and network restrictions
4. **Gmail Account** - Ensure 2FA is enabled and app password is fresh
5. **Quota** - Check if you've exceeded daily sending limits

### Get Help:
- Check the application logs in the console
- Look at the warning message when creating employee
- The warning will show you the exact error and credentials
- You can manually provide the credentials to the employee

---

## ? Success Indicators

When email is working correctly, you'll see:

**In Logs:**
```
Attempting to send email to user@example.com via smtp.gmail.com:587
Connecting to SMTP server: smtp.gmail.com:587
Authenticating with username: samarthbhalala@gmail.com
Sending email to: user@example.com
? Email sent successfully to user@example.com
```

**In UI:**
```
Success: Employee created successfully! Login ID: OIJD20230001. 
Credentials have been sent to user@example.com
```

**In Email Inbox:**
```
Subject: HRMS - Your Login Credentials
From: HRMS System <samarthbhalala@gmail.com>

[Formatted HTML email with credentials]
```

---

**Last Updated:** 2024
