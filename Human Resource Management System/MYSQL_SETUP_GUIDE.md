# MySQL Database Setup Guide for HRMS (Network Configuration)

This guide will help you set up the HRMS application to work with MySQL using a network MySQL server at `192.168.0.114`.

## Prerequisites

1. **MySQL Server** - Must be running and accessible at `192.168.0.114:3306`
2. **.NET 10 SDK** - Should already be installed
3. **Entity Framework Tools** - Install globally if not present:
   ```bash
   dotnet tool install --global dotnet-ef
   ```
4. **Network Access** - Ensure you can reach the MySQL server at 192.168.0.114
5. **MySQL Permissions** - Root user must have remote access enabled

## Quick Setup

### Option 1: Automatic Setup (Recommended)

Run one of the setup scripts:

**Windows Batch Script:**
```cmd
Setup-MySQL-Database.bat
```

**PowerShell Script:**
```powershell
.\Setup-MySQL-Database.ps1
```

### Option 2: Manual Setup

1. **Test MySQL Connection**
   ```bash
   mysql -u root -h 192.168.0.114 -P 3306 -e "SELECT 1"
   ```

2. **Create Database**
   - Open phpMyAdmin: http://192.168.0.114/phpmyadmin/
   - Create new database: `hrms_database`
   - Set charset to `utf8mb4` and collation to `utf8mb4_unicode_ci`

3. **Restore NuGet Packages**
   ```bash
   dotnet restore
   ```

4. **Create and Apply Migrations**
   ```bash
   # Remove old migrations if any
   rm -rf Migrations/
   
   # Create new migration
   dotnet ef migrations add InitialCreateMySQL
   
   # Apply to database
   dotnet ef database update
   ```

5. **Run Application**
   ```bash
   dotnet run
   ```

## Database Configuration

The application is configured to connect to MySQL with these settings:

- **Server:** 192.168.0.114
- **Port:** 3306
- **Database:** hrms_database
- **Username:** root
- **Password:** (empty)

### Custom MySQL Settings

If you need to use different MySQL settings, update `appsettings.json`:

```json
{
  "DatabaseSettings": {
    "UseCustomServer": true,
    "MySqlHost": "192.168.0.114",
    "MySqlPort": 3306,
    "DatabaseName": "hrms_database",
    "Username": "your-username",
    "Password": "your-password"
  }
}
```

Or update the connection string directly:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.0.114;Port=3306;Database=hrms_database;Uid=your-user;Pwd=your-password;"
  }
}
```

## Default Login Credentials

After successful setup, use these credentials to log into the system:

- **Email:** admin@hrms.com
- **Password:** admin123

## Managing the Database

### Using phpMyAdmin
- **Main URL:** http://192.168.0.114/phpmyadmin/
- **Direct Database URL:** http://192.168.0.114/phpmyadmin/index.php?route=/database/structure&db=hrms_database
- **Username:** root
- **Password:** (leave empty)

### Using MySQL Command Line
```bash
mysql -u root -h 192.168.0.114 -P 3306 hrms_database
```

### Using MySQL Workbench
- **Hostname:** 192.168.0.114
- **Port:** 3306
- **Username:** root
- **Password:** (empty)

## Troubleshooting

### Network Connection Issues
1. **Ping the server:**
   ```bash
   ping 192.168.0.114
   ```

2. **Test port connectivity:**
   ```bash
   telnet 192.168.0.114 3306
   ```

3. **Check MySQL remote access:**
   - Ensure MySQL is configured to accept remote connections
   - Verify user permissions allow connections from your IP
   - Check firewall settings on the MySQL server

### MySQL Connection Issues
1. Verify MySQL server is running at 192.168.0.114
2. Check if port 3306 is accessible
3. Ensure root user has remote access privileges:
   ```sql
   GRANT ALL PRIVILEGES ON *.* TO 'root'@'%' WITH GRANT OPTION;
   FLUSH PRIVILEGES;
   ```

### Migration Issues
1. Delete the `Migrations` folder
2. Recreate migrations:
   ```bash
   dotnet ef migrations add InitialCreateMySQL
   ```
3. Apply migrations:
   ```bash
   dotnet ef database update
   ```

### Package Issues
1. Clear NuGet cache:
   ```bash
   dotnet nuget locals all --clear
   ```
2. Restore packages:
   ```bash
   dotnet restore
   ```

### Database Already Exists
If you get errors about existing database objects:
```sql
DROP DATABASE hrms_database;
CREATE DATABASE hrms_database CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

## Security Considerations

When using a network MySQL server:

1. **Use strong passwords** for MySQL users
2. **Restrict IP access** to only necessary machines
3. **Enable SSL/TLS** for MySQL connections if possible
4. **Regular backups** of the database
5. **Monitor access logs** for suspicious activity

## What's Changed for Network Configuration

The following changes were made to work with the network MySQL server:

1. **Connection Strings** - Updated to use `192.168.0.114` instead of `localhost`
2. **Setup Scripts** - Modified to connect to network server
3. **Database Settings** - Updated default host to network IP
4. **Documentation** - Added network-specific troubleshooting

## Verification

After setup, you can verify the installation:

1. **Check Database Connection:**
   ```bash
   mysql -u root -h 192.168.0.114 -P 3306 -e "USE hrms_database; SHOW TABLES;"
   ```

2. **Check Admin User:**
   ```sql
   USE hrms_database;
   SELECT * FROM Users WHERE Email = 'admin@hrms.com';
   ```

3. **Check Sample Employees:**
   ```sql
   USE hrms_database;
   SELECT * FROM Employees;
   ```

4. **Test Application:**
   - Start with `dotnet run`
   - Navigate to https://localhost:7037
   - Login with admin credentials

The application should now be fully functional with the network MySQL database backend!