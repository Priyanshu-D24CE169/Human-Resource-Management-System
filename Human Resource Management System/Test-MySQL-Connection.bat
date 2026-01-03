@echo off
echo ====================================
echo MySQL Network Connectivity Test
echo ====================================
echo.

echo Testing connection to MySQL server at 192.168.0.114...
echo.

echo Step 1: Testing network connectivity...
ping -n 1 192.168.0.114 >nul 2>&1
if %errorlevel% equ 0 (
    echo ? Network ping successful to 192.168.0.114
) else (
    echo ? Network ping failed to 192.168.0.114
    echo   Check your network connection and server availability
    goto :end
)

echo.
echo Step 2: Testing MySQL port accessibility...
echo Testing port 3306...
powershell -Command "& {try { $tcp = New-Object Net.Sockets.TcpClient; $tcp.Connect('192.168.0.114', 3306); $tcp.Close(); Write-Host '? Port 3306 is accessible'; exit 0 } catch { Write-Host '? Port 3306 is not accessible'; exit 1 }}"
if %errorlevel% neq 0 (
    echo   Make sure MySQL server is running and port 3306 is open
    goto :end
)

echo.
echo Step 3: Testing MySQL authentication...
mysql -u root -h 192.168.0.114 -P 3306 -e "SELECT 'Authentication successful' as Status, VERSION() as MySQL_Version;" 2>nul
if %errorlevel% equ 0 (
    echo ? MySQL authentication successful
) else (
    echo ? MySQL authentication failed
    echo   Check MySQL user permissions for remote access
    goto :end
)

echo.
echo Step 4: Testing database existence...
mysql -u root -h 192.168.0.114 -P 3306 -e "SHOW DATABASES LIKE 'hrms_database';" 2>nul | findstr "hrms_database" >nul
if %errorlevel% equ 0 (
    echo ? Database 'hrms_database' exists
) else (
    echo ! Database 'hrms_database' not found (will be created during setup)
)

echo.
echo ====================================
echo CONNECTIVITY TEST COMPLETE
echo ====================================
echo.
echo All basic connectivity tests passed!
echo.
echo phpMyAdmin URLs:
echo - Main: http://192.168.0.114/phpmyadmin/
echo - Database: http://192.168.0.114/phpmyadmin/index.php?route=/database/structure^&db=hrms_database
echo.
echo You can now run the setup script:
echo Setup-MySQL-Database.bat
echo.

:end
pause