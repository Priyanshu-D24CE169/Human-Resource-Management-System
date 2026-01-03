# Database Migration and Setup Guide

## Prerequisites

### For SQL Server Express (Recommended for Intranet Sharing)
1. **Download and Install SQL Server Express** (if not already installed)
   ```
   https://www.microsoft.com/en-us/sql-server/sql-server-downloads
   ```
   - Choose "Express" edition
   - During installation, enable "Named Pipes" and "TCP/IP" protocols
   - Set SQL Server Browser to "Automatic" start

2. **Install SQL Server Management Studio (SSMS)**
   ```
   https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms
   ```

### For Development Only (LocalDB)
- Comes with Visual Studio, no separate installation needed

## Step 1: Configure SQL Server for Network Access

### Enable SQL Server Network Access
1. Open **SQL Server Configuration Manager**
2. Navigate to **SQL Server Network Configuration** > **Protocols for SQLEXPRESS**
3. Enable **TCP/IP** and **Named Pipes**
4. Right-click **TCP/IP** > **Properties**
5. In **IP Addresses** tab, find **IPAll** section
6. Set **TCP Port** to **1433** (or note the port number)
7. Restart **SQL Server (SQLEXPRESS)** service

### Configure Windows Firewall
Run as Administrator in Command Prompt:
```cmd
netsh advfirewall firewall add rule name="SQL Server Express" dir=in action=allow protocol=TCP localport=1433
netsh advfirewall firewall add rule name="SQL Browser" dir=in action=allow protocol=UDP localport=1434
```

## Step 2: Run Database Migration

### Option A: Automatic Migration (Recommended)
The application will automatically create the database when you run it:

```powershell
# Navigate to project directory
cd "A:\Projects\odoo hackathon\Human Resource Management System"

# Restore packages
dotnet restore

# Run the application
dotnet run
```

The application will:
- Create the database if it doesn't exist
- Seed initial data (admin user and sample employees)
- Display network URLs for intranet access

### Option B: Manual Migration using EF Core Tools
```powershell
# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

## Step 3: Network Access Setup

### Find Your Local IP Address
```powershell
ipconfig | findstr "IPv4"
```

### Configure Application for Network Access
The application is already configured to listen on all network interfaces.

When you run the application, it will display:
- Local access: `https://localhost:7037`
- Network access: `https://[YOUR-IP]:7037`

### Share Database Connection
Other devices on your network can access:
- **Web Application**: `https://[YOUR-IP]:7037`
- **Database**: Connect to `[YOUR-IP]\SQLEXPRESS` using SSMS

## Step 4: Default Login Credentials

```
Email: admin@hrms.com
Password: admin123
```

## Step 5: Database Connection Strings

### For Intranet Sharing (SQL Server Express)
```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=HrmsDatabase;Integrated Security=true;MultipleActiveResultSets=true;TrustServerCertificate=true;"
```

### For Remote Access (from other machines)
```json
"DefaultConnection": "Server=[YOUR-IP]\\SQLEXPRESS;Database=HrmsDatabase;Integrated Security=true;MultipleActiveResultSets=true;TrustServerCertificate=true;"
```

## Troubleshooting

### Common Issues and Solutions

#### 1. SQL Server Express Not Found
**Error**: "A network-related or instance-specific error occurred..."
**Solution**: 
- Ensure SQL Server Express is installed and running
- Check SQL Server Configuration Manager
- Verify Windows Services: SQL Server (SQLEXPRESS) is running

#### 2. Network Access Denied
**Error**: Connection timeout from other machines
**Solution**:
- Check Windows Firewall settings
- Ensure TCP/IP is enabled in SQL Server Configuration
- Verify the computer is connected to the same network

#### 3. Database Permission Issues
**Error**: "Login failed" or "Access denied"
**Solution**:
- Ensure the application is running with appropriate permissions
- For remote access, configure SQL Server authentication

#### 4. Automatic Fallback to LocalDB
If SQL Server Express is not available, the application will automatically fall back to LocalDB for development purposes.

## Network Sharing Commands

### Share Database Access (Advanced)
If you need remote database connections, create a SQL Server user:

```sql
-- Connect to SSMS as Administrator
USE master;
GO

-- Create login
CREATE LOGIN hrms_user WITH PASSWORD = 'HrmsPass123!';
GO

-- Use the HRMS database
USE HrmsDatabase;
GO

-- Create user and grant permissions
CREATE USER hrms_user FOR LOGIN hrms_user;
ALTER ROLE db_datareader ADD MEMBER hrms_user;
ALTER ROLE db_datawriter ADD MEMBER hrms_user;
GO
```

### Remote Connection String (for other applications)
```json
"RemoteConnection": "Server=[HOST-IP]\\SQLEXPRESS;Database=HrmsDatabase;User Id=hrms_user;Password=HrmsPass123!;TrustServerCertificate=true;"
```

## Security Notes

1. **For Production**: Change default passwords and enable proper authentication
2. **Network Security**: Consider VPN for secure remote access
3. **Database Security**: Implement proper user roles and permissions
4. **SSL Certificates**: Use proper SSL certificates for HTTPS in production

## Verification Steps

1. **Local Verification**:
   - Open browser: `https://localhost:7037`
   - Login with admin credentials
   - Check database in SSMS: Connect to `(local)\SQLEXPRESS`

2. **Network Verification**:
   - From another device on same network
   - Open browser: `https://[HOST-IP]:7037`
   - Verify login and functionality

3. **Database Verification**:
   - Open SSMS on another machine
   - Connect to: `[HOST-IP]\SQLEXPRESS`
   - Browse HrmsDatabase tables