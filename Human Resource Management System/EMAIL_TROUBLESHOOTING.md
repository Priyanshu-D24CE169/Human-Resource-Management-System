# ?? Email Troubleshooting Guide - HRMS System

## Current Email Configuration ?
- **SMTP Host**: smtp.gmail.com
- **SMTP Port**: 587
- **SSL Enabled**: Yes
- **Sender Email**: samarthbhalala@gmail.com
- **App Password**: ntem pcsd xqjg bpox

## ?? Email Issues Diagnostic Steps

### Step 1: Quick Test (Do This First!)
1. **Navigate to**: `http://localhost:5194/Employee/TestEmail`
2. **Check the JSON response** for detailed error information
3. **Check application logs** in Visual Studio Output window

### Step 2: Verify Gmail Settings
1. **Login to Gmail**: samarthbhalala@gmail.com
2. **Check 2-Factor Authentication**: Must be enabled for app passwords
3. **Verify App Password**: 
   - Go to Google Account ? Security ? App Passwords
   - Make sure "ntem pcsd xqjg bpox" is still valid
   - If needed, generate a new 16-character app password

### Step 3: Network/Security Check
1. **Windows Firewall**: May block outgoing SMTP connections
2. **Antivirus Software**: Often blocks SMTP on port 587
3. **Router/ISP**: Some ISPs block SMTP ports

## ?? Common Issues & Solutions

### Issue 1: "Authentication Failed"
**Cause**: Gmail credentials or app password incorrect
**Solutions**:
- Verify 2FA is enabled on Gmail account
- Generate new app password: Google Account ? Security ? App passwords
- Replace old password with new 16-character password in appsettings

### Issue 2: "Connection Timeout"
**Cause**: Firewall or network blocking SMTP
**Solutions**:
- Temporarily disable Windows Firewall
- Check antivirus real-time protection settings
- Try different SMTP port (465 with SSL)

### Issue 3: "Server Does Not Support TLS"
**Cause**: SSL/TLS configuration issue
**Solutions**:
- Use port 465 with SSL instead of 587 with TLS
- Update appsettings.json EmailSettings

### Issue 4: "Mailbox Unavailable"
**Cause**: Recipient email validation or limits
**Solutions**:
- Try sending to a different email address
- Check Gmail sending limits (500 emails/day)

## ?? Alternative SMTP Configuration

If Gmail continues to fail, try this configuration:

```json
"EmailSettings": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": "465",
  "EnableSsl": "true",
  "SenderEmail": "samarthbhalala@gmail.com",
  "SenderPassword": "ntem pcsd xqjg bpox",
  "SenderName": "Odoo India HRMS"
}
```

## ?? Manual Email Test Steps

### Test 1: Browser Test
Navigate to: `http://localhost:5194/Employee/TestEmail`

**Expected Success Response**:
```json
{"success": true, "message": "? Test email sent successfully!"}
```

**Expected Error Response**:
```json
{"success": false, "message": "? Failed to send test email. Check logs."}
```

### Test 2: Create Employee Test
1. Use the employee creation form
2. Fill in ALL required fields
3. Use a **real email address** you can check
4. Click "Create Employee & Send Email"
5. Check for success/warning message

## ?? Detailed Debugging

### Check Application Logs
1. **Visual Studio Output Window**
2. **Debug ? Windows ? Output**
3. **Show output from: Debug**

**Look for these log entries**:
- `?? Starting email send process to: [email]`
- `?? SMTP Configuration: Host: smtp.gmail.com`
- `? Email sent successfully` OR `? SMTP Error: [details]`

### Common Log Errors:

**"SMTP password is empty or null!"**
- Fix: Check appsettings.json EmailSettings

**"SmtpException: Authentication failed"**
- Fix: Regenerate Gmail app password

**"SmtpException: Connection timeout"**
- Fix: Check firewall, try port 465

**"SmtpException: Server does not support secure connections"**
- Fix: Change EnableSsl to false or use port 465

## ? Quick Fixes

### Fix 1: Update Email Configuration
```json
"EmailSettings": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": "587",
  "EnableSsl": "true",
  "SenderEmail": "samarthbhalala@gmail.com",
  "SenderPassword": "[NEW_16_CHAR_APP_PASSWORD]",
  "SenderName": "Odoo India HRMS"
}
```

### Fix 2: Generate New Gmail App Password
1. **Gmail** ? **Google Account** ? **Security**
2. **2-Step Verification** (must be enabled)
3. **App passwords** ? **Generate** ? **Select app: Other**
4. **Name**: "HRMS System"
5. **Copy 16-character password**
6. **Update appsettings.json**

### Fix 3: Test with Different Email Provider
```json
"EmailSettings": {
  "SmtpHost": "smtp.outlook.com",
  "SmtpPort": "587",
  "EnableSsl": "true",
  "SenderEmail": "your-outlook-email@outlook.com",
  "SenderPassword": "your-password",
  "SenderName": "Odoo India HRMS"
}
```

## ?? Expected Working Flow

### When Everything Works:
1. **Employee Creation**: Form submits successfully
2. **Employee Code**: Generates automatically (OIJACH20260001)
3. **Database**: Employee saved with registration token
4. **Email Sending**: SMTP connects and sends
5. **Success Message**: "Employee created successfully! Welcome email sent to [email]"
6. **Email Delivered**: Recipient gets welcome email with registration link

### When Email Fails:
1. **Employee Creation**: Still succeeds
2. **Warning Message**: "Employee created successfully, but welcome email failed to send"
3. **Manual Fix**: Use "Resend Registration" button later

## ?? Final Verification Steps

### Test the Complete Flow:
1. ? **Login**: admin@hrms.com / admin123
2. ? **Navigate**: Employee Management ? Add New Employee  
3. ? **Fill Form**: Use real email address
4. ? **Submit**: Click "Create Employee & Send Email"
5. ? **Check Result**: Should see success message
6. ? **Check Email**: Welcome email should arrive within 1-2 minutes
7. ? **Test Registration**: Click link in email to complete registration

---

## ?? If All Else Fails

**Quick Workaround**: 
1. Employee creation will still work (database saves correctly)
2. Employee codes generate properly
3. Manual registration links can be shared
4. Fix email later and use "Resend Registration" feature

**Contact Support**:
- Check Gmail account settings
- Verify 2-factor authentication is enabled
- Try generating a fresh app password
- Test with a different email provider

The HRMS system will work fine even without emails - they're just for convenience! ??