@echo off
echo ====================================
echo HRMS MySQL Database Setup (Network)
echo ====================================
echo.

echo This script will:
echo 1. Create the MySQL database for HRMS
echo 2. Install required NuGet packages
echo 3. Create and run migrations
echo 4. Seed initial data
echo.

echo Prerequisites:
echo - MySQL server must be running at 192.168.0.114
echo - MySQL service must be accessible on port 3306
echo - You must have access to the MySQL server
echo.

set /p continue="Continue with setup? (Y/N): "
if /i "%continue%" neq "Y" exit /b

echo.
echo Step 1: Checking MySQL connection at 192.168.0.114...
echo Attempting to connect to MySQL at 192.168.0.114:3306...

mysql -u root -h 192.168.0.114 -P 3306 -e "SELECT 'MySQL connection successful' as Status;" 2>nul
if %errorlevel% neq 0 (
    echo ERROR: Cannot connect to MySQL. Please ensure:
    echo - MySQL server is running at 192.168.0.114
    echo - MySQL service is accessible on port 3306
    echo - Network connectivity is available
    echo - User 'root' has remote access permissions
    echo.
    echo You can also access phpMyAdmin at: http://192.168.0.114/phpmyadmin/
    pause
    exit /b 1
)

echo MySQL connection successful!
echo.

echo Step 2: Creating database...
mysql -u root -h 192.168.0.114 -P 3306 -e "CREATE DATABASE IF NOT EXISTS hrms_database CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
if %errorlevel% equ 0 (
    echo Database 'hrms_database' created successfully!
) else (
    echo Warning: Database creation might have failed, but continuing...
)

echo.
echo Step 3: Installing/Updating NuGet packages...
dotnet restore
if %errorlevel% neq 0 (
    echo ERROR: Failed to restore NuGet packages
    pause
    exit /b 1
)

echo.
echo Step 4: Removing old migrations (if any)...
if exist "Migrations" (
    rmdir /s /q "Migrations"
    echo Old migrations removed.
)

echo.
echo Step 5: Creating new migration...
dotnet ef migrations add InitialCreateMySQL
if %errorlevel% neq 0 (
    echo ERROR: Failed to create migration
    echo Make sure Entity Framework tools are installed:
    echo dotnet tool install --global dotnet-ef
    pause
    exit /b 1
)

echo.
echo Step 6: Updating database with migrations...
dotnet ef database update
if %errorlevel% neq 0 (
    echo ERROR: Failed to update database
    pause
    exit /b 1
)

echo.
echo ====================================
echo DATABASE SETUP COMPLETE!
echo ====================================
echo.
echo Database Details:
echo - Server: 192.168.0.114
echo - Port: 3306
echo - Database: hrms_database
echo - Username: root
echo - Password: (empty)
echo.
echo Default Admin Credentials:
echo - Email: admin@hrms.com
echo - Password: admin123
echo.
echo You can manage the database using:
echo - phpMyAdmin: http://192.168.0.114/phpmyadmin/
echo - Direct database URL: http://192.168.0.114/phpmyadmin/index.php?route=/database/structure&db=hrms_database
echo - MySQL Workbench (if installed)
echo.
echo The application should now be ready to run!
echo Use 'dotnet run' to start the application.
echo.
pause