@echo off
echo ================================
echo HRMS Intranet SQL Server Setup
echo ================================
echo.

set /p SERVER_IP="Enter SQL Server IP Address (e.g., 192.168.1.100): "
set /p SERVER_HOSTNAME="Enter SQL Server Hostname (e.g., DESKTOP-ABC123): "
set /p SQL_USERNAME="Enter SQL Server Username (e.g., sa): "
set /p SQL_PASSWORD="Enter SQL Server Password: "

echo.
echo Updating configuration files...
echo.

:: Update appsettings.Development.json
powershell -Command "(Get-Content 'appsettings.Development.json') -replace 'YOUR_IP_ADDRESS', '%SERVER_IP%' | Set-Content 'appsettings.Development.json'"
powershell -Command "(Get-Content 'appsettings.Development.json') -replace 'YOUR_HOSTNAME', '%SERVER_HOSTNAME%' | Set-Content 'appsettings.Development.json'"
powershell -Command "(Get-Content 'appsettings.Development.json') -replace 'YOUR_SQL_USERNAME', '%SQL_USERNAME%' | Set-Content 'appsettings.Development.json'"
powershell -Command "(Get-Content 'appsettings.Development.json') -replace 'YOUR_SQL_PASSWORD', '%SQL_PASSWORD%' | Set-Content 'appsettings.Development.json'"

:: Update appsettings.json
powershell -Command "(Get-Content 'appsettings.json') -replace 'YOUR_IP_ADDRESS', '%SERVER_IP%' | Set-Content 'appsettings.json'"
powershell -Command "(Get-Content 'appsettings.json') -replace 'YOUR_HOSTNAME', '%SERVER_HOSTNAME%' | Set-Content 'appsettings.json'"
powershell -Command "(Get-Content 'appsettings.json') -replace 'YOUR_SQL_USERNAME', '%SQL_USERNAME%' | Set-Content 'appsettings.json'"
powershell -Command "(Get-Content 'appsettings.json') -replace 'YOUR_SQL_PASSWORD', '%SQL_PASSWORD%' | Set-Content 'appsettings.json'"

echo Configuration files updated successfully!
echo.
echo IMPORTANT: Make sure your SQL Server is configured for:
echo 1. SQL Server Authentication mode
echo 2. TCP/IP protocol enabled on port 1433
echo 3. Windows Firewall exception for port 1433
echo 4. User '%SQL_USERNAME%' has proper database permissions
echo.
echo Run the application to test the connection.
echo Check the console output to see which connection method succeeded.
echo.
pause