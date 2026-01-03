# ?? HRMS Database Migration & Network Setup - COMPLETE GUIDE

## ?? Quick Setup Instructions

### Option 1: Automatic Setup (Recommended)
1. **Run as Administrator**: Right-click `Quick-Setup.bat` ? "Run as administrator"
2. **Follow the prompts** - the script will handle everything automatically
3. **Access the application** at displayed URLs

### Option 2: Manual Setup
1. Open PowerShell as Administrator
2. Navigate to project folder
3. Run: `.\Setup-Database.ps1`

### Option 3: Developer Setup
```powershell
# Restore packages and run
dotnet restore
dotnet run
```

## ?? Network Access Information

Once running, your HRMS will be accessible on:

| Access Type | URL Format | Example |
|-------------|------------|---------|
| **Local Machine** | `https://localhost:7037` | `https://localhost:7037` |
| **Same WiFi Network** | `https://[YOUR-IP]:7037` | `https://192.168.1.100:7037` |
| **Intranet Access** | `https://[COMPUTER-NAME]:7037` | `https://DESKTOP-ABC123:7037` |

## ?? Default Login Credentials

```
Email: admin@hrms.com
Password: admin123
```

## ??? Database Information

| Setting | Value |
|---------|--------|
| **Database Name** | `HrmsDatabase` |
| **Server Instance** | `.\SQLEXPRESS` (or LocalDB fallback) |
| **Connection** | Integrated Windows Authentication |
| **Auto-Creation** | ? Yes - Created on first run |
| **Seeded Data** | ? Admin user + 3 sample employees |

## ?? Accessing from Other Devices

### On Same WiFi Network:
1. **Find Host Computer IP**: The application displays this when it starts
2. **Open browser** on any device (phone, tablet, laptop)
3. **Navigate to**: `https://[HOST-IP]:7037`
4. **Accept security warning** (self-signed certificate)
5. **Login** with admin credentials

### Example Access URLs:
- From Windows laptop: `https://192.168.1.100:7037`
- From Android phone: `https://192.168.1.100:7037`
- From iPhone/iPad: `https://192.168.1.100:7037`

## ??? Database Management

### Access with SQL Server Management Studio (SSMS):
1. **Server Name**: `(local)\SQLEXPRESS` or `.\SQLEXPRESS`
2. **Authentication**: Windows Authentication
3. **Database**: `HrmsDatabase`

### Backup Database:
```sql
BACKUP DATABASE [HrmsDatabase] 
TO DISK = 'C:\Backup\HrmsDatabase.bak'
```

### Restore Database:
```sql
RESTORE DATABASE [HrmsDatabase] 
FROM DISK = 'C:\Backup\HrmsDatabase.bak'
```

## ?? Troubleshooting

### Common Issues & Solutions

#### ? "Cannot connect to database"
**Solutions:**
1. Install SQL Server Express
2. Start SQL Server service: `net start MSSQL$SQLEXPRESS`
3. Application will auto-fallback to LocalDB

#### ? "Cannot access from other devices"
**Solutions:**
1. Check Windows Firewall settings
2. Ensure devices are on same network
3. Try HTTP instead: `http://[IP]:5194`

#### ? "Site can't be reached"
**Solutions:**
1. Verify the host computer IP address
2. Check if application is still running
3. Try different port or HTTP version

#### ? "Your connection is not private"
**Solutions:**
1. Click "Advanced" ? "Proceed to [IP] (unsafe)"
2. This is normal for development certificates
3. For production, use proper SSL certificates

## ?? What's Included

### ? Database Tables Created:
- **Users** - Authentication and user management
- **Employees** - Employee information
- **Attendance** - Time tracking
- **Payroll** - Salary and payment records
- **LeaveRequests** - Leave management

### ? Sample Data:
- 1 Admin user
- 3 Sample employees
- Ready-to-use system

### ? Network Features:
- HTTPS encryption
- Cross-device compatibility
- Session management
- Auto-migration

## ?? Next Steps

1. **Change Default Password**: Login and update admin password
2. **Add Employees**: Use Employee ? Create to add your team
3. **Configure Settings**: Customize as needed
4. **Backup Strategy**: Set up regular database backups
5. **Production Setup**: For live use, configure proper SSL certificates

## ?? Support Commands

### Check Application Status:
```powershell
# See if application is running
netstat -an | findstr :7037
```

### Restart Services:
```powershell
# Restart SQL Server
net stop MSSQL$SQLEXPRESS
net start MSSQL$SQLEXPRESS
```

### View Application Logs:
- Check console output where `dotnet run` was executed
- Application logs are displayed in real-time

---

## ?? Your HRMS is Now Ready!

The system is configured for:
- ? Local development
- ? Network sharing
- ? Database auto-creation
- ? Multi-device access
- ? Production-ready foundation

**Enjoy your Human Resource Management System!**