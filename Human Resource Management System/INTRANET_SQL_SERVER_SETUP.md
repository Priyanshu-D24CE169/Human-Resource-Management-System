# Intranet SQL Server Configuration Guide

This guide will help you configure the application to connect to an intranet SQL Server instance.

## Step 1: Update Configuration Files

Replace the placeholder values in your `appsettings.Development.json` (or `appsettings.json` for production) file:

### Replace These Values:
- `YOUR_IP_ADDRESS` ? Your SQL Server machine's IP address (e.g., `192.168.1.100`)
- `YOUR_HOSTNAME` ? Your SQL Server machine's hostname (e.g., `DESKTOP-ABC123`)
- `YOUR_SQL_USERNAME` ? Your SQL Server username (e.g., `sa` or `hrms_user`)
- `YOUR_SQL_PASSWORD` ? Your SQL Server password

### Example Configuration:
```json
{
  "DatabaseSettings": {
    "UseIntranetServer": true,
    "PreferHostnameOverIP": false,
    "SqlServerIP": "192.168.1.100",
    "SqlServerHostname": "DESKTOP-ABC123",
    "SqlUsername": "sa",
    "SqlPassword": "YourStrongPassword123!"
  }
}
```

## Step 2: SQL Server Configuration

Ensure your SQL Server instance is configured for network access:

### Enable SQL Server Authentication:
1. Open SQL Server Management Studio (SSMS)
2. Connect to your SQL Server instance
3. Right-click on the server name ? Properties
4. Go to Security tab
5. Select "SQL Server and Windows Authentication mode"
6. Click OK and restart SQL Server service

### Enable TCP/IP Protocol:
1. Open SQL Server Configuration Manager
2. Expand "SQL Server Network Configuration"
3. Click on "Protocols for [INSTANCE_NAME]"
4. Right-click "TCP/IP" ? Enable
5. Right-click "TCP/IP" ? Properties
6. Go to "IP Addresses" tab
7. Scroll to "IPAll" section
8. Set "TCP Port" to `1433` (default) or your custom port
9. Restart SQL Server service

### Configure Windows Firewall:
1. Open Windows Firewall with Advanced Security
2. Create new Inbound Rule
3. Select Port ? TCP ? Specific local ports ? 1433
4. Allow the connection
5. Apply to Domain, Private, and Public (as needed)
6. Name the rule "SQL Server"

## Step 3: Create SQL Server Login (if needed)

If you don't have a SQL Server user, create one:

```sql
-- Connect to master database
USE master;
GO

-- Create login
CREATE LOGIN hrms_user WITH PASSWORD = 'YourStrongPassword123!';
GO

-- Create user in HrmsDatabase
USE HrmsDatabase;
GO
CREATE USER hrms_user FOR LOGIN hrms_user;
GO

-- Grant permissions
ALTER ROLE db_owner ADD MEMBER hrms_user;
GO
```

## Step 4: Test Configuration

The application will automatically test connections in this order:
1. Intranet SQL Server (Hostname or IP, based on preference)
2. Local SQL Server Express
3. LocalDB

Check the console output when starting the application to see which connection succeeded.

## Step 5: Configuration Options

### DatabaseSettings Options:

- **UseIntranetServer**: `true` to enable intranet server, `false` to use local only
- **PreferHostnameOverIP**: `true` to try hostname first, `false` to try IP first
- **SqlServerIP**: IP address of your SQL Server machine
- **SqlServerHostname**: Hostname of your SQL Server machine  
- **SqlUsername**: SQL Server authentication username
- **SqlPassword**: SQL Server authentication password

## Troubleshooting

### Common Issues:

1. **Connection Timeout**: 
   - Check if SQL Server service is running
   - Verify firewall settings
   - Test connectivity with `telnet [IP] 1433`

2. **Login Failed**:
   - Verify SQL Server authentication is enabled
   - Check username/password
   - Ensure user has proper permissions

3. **Server Not Found**:
   - Verify IP address/hostname is correct
   - Check network connectivity
   - Ensure TCP/IP protocol is enabled

4. **Database Not Found**:
   - The application will create the database automatically
   - Ensure the SQL user has `dbcreator` rights or manually create the database

### Testing Connection:
You can test the SQL Server connection using SSMS or command line:
```bash
sqlcmd -S [IP_ADDRESS],1433 -U [USERNAME] -P [PASSWORD]
```

## Security Notes

- Use strong passwords for SQL Server authentication
- Consider creating a dedicated database user instead of using `sa`
- Limit network access to trusted machines only
- Use encrypted connections in production environments