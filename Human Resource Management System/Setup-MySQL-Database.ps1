#!/usr/bin/env pwsh

Write-Host "====================================" -ForegroundColor Green
Write-Host "HRMS MySQL Database Setup (Network)" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green
Write-Host ""

Write-Host "This script will:" -ForegroundColor Yellow
Write-Host "1. Create the MySQL database for HRMS" -ForegroundColor Yellow
Write-Host "2. Install required NuGet packages" -ForegroundColor Yellow
Write-Host "3. Create and run migrations" -ForegroundColor Yellow
Write-Host "4. Seed initial data" -ForegroundColor Yellow
Write-Host ""

Write-Host "Prerequisites:" -ForegroundColor Cyan
Write-Host "- MySQL server must be running at 192.168.0.114" -ForegroundColor Cyan
Write-Host "- MySQL service must be accessible on port 3306" -ForegroundColor Cyan
Write-Host "- You must have access to the MySQL server" -ForegroundColor Cyan
Write-Host ""

$continue = Read-Host "Continue with setup? (Y/N)"
if ($continue -notmatch '^[Yy]') {
    exit
}

Write-Host ""
Write-Host "Step 1: Checking MySQL connection at 192.168.0.114..." -ForegroundColor Blue
Write-Host "Attempting to connect to MySQL at 192.168.0.114:3306..." -ForegroundColor Gray

try {
    $result = & mysql -u root -h 192.168.0.114 -P 3306 -e "SELECT 'MySQL connection successful' as Status;" 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "MySQL connection failed"
    }
    Write-Host "MySQL connection successful!" -ForegroundColor Green
}
catch {
    Write-Host "ERROR: Cannot connect to MySQL. Please ensure:" -ForegroundColor Red
    Write-Host "- MySQL server is running at 192.168.0.114" -ForegroundColor Red
    Write-Host "- MySQL service is accessible on port 3306" -ForegroundColor Red
    Write-Host "- Network connectivity is available" -ForegroundColor Red
    Write-Host "- User 'root' has remote access permissions" -ForegroundColor Red
    Write-Host ""
    Write-Host "You can also access phpMyAdmin at: http://192.168.0.114/phpmyadmin/" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host ""
Write-Host "Step 2: Creating database..." -ForegroundColor Blue
try {
    & mysql -u root -h 192.168.0.114 -P 3306 -e "CREATE DATABASE IF NOT EXISTS hrms_database CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Database 'hrms_database' created successfully!" -ForegroundColor Green
    } else {
        Write-Host "Warning: Database creation might have failed, but continuing..." -ForegroundColor Yellow
    }
}
catch {
    Write-Host "Warning: Database creation encountered an issue, but continuing..." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Step 3: Installing/Updating NuGet packages..." -ForegroundColor Blue
try {
    & dotnet restore
    if ($LASTEXITCODE -ne 0) {
        throw "NuGet restore failed"
    }
    Write-Host "NuGet packages restored successfully!" -ForegroundColor Green
}
catch {
    Write-Host "ERROR: Failed to restore NuGet packages" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host ""
Write-Host "Step 4: Removing old migrations (if any)..." -ForegroundColor Blue
if (Test-Path "Migrations") {
    Remove-Item "Migrations" -Recurse -Force
    Write-Host "Old migrations removed." -ForegroundColor Green
}

Write-Host ""
Write-Host "Step 5: Creating new migration..." -ForegroundColor Blue
try {
    & dotnet ef migrations add InitialCreateMySQL
    if ($LASTEXITCODE -ne 0) {
        throw "Migration creation failed"
    }
    Write-Host "Migration created successfully!" -ForegroundColor Green
}
catch {
    Write-Host "ERROR: Failed to create migration" -ForegroundColor Red
    Write-Host "Make sure Entity Framework tools are installed:" -ForegroundColor Yellow
    Write-Host "dotnet tool install --global dotnet-ef" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host ""
Write-Host "Step 6: Updating database with migrations..." -ForegroundColor Blue
try {
    & dotnet ef database update
    if ($LASTEXITCODE -ne 0) {
        throw "Database update failed"
    }
    Write-Host "Database updated successfully!" -ForegroundColor Green
}
catch {
    Write-Host "ERROR: Failed to update database" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host ""
Write-Host "====================================" -ForegroundColor Green
Write-Host "DATABASE SETUP COMPLETE!" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Database Details:" -ForegroundColor Cyan
Write-Host "- Server: 192.168.0.114" -ForegroundColor White
Write-Host "- Port: 3306" -ForegroundColor White
Write-Host "- Database: hrms_database" -ForegroundColor White
Write-Host "- Username: root" -ForegroundColor White
Write-Host "- Password: (empty)" -ForegroundColor White
Write-Host ""
Write-Host "Default Admin Credentials:" -ForegroundColor Cyan
Write-Host "- Email: admin@hrms.com" -ForegroundColor White
Write-Host "- Password: admin123" -ForegroundColor White
Write-Host ""
Write-Host "You can manage the database using:" -ForegroundColor Cyan
Write-Host "- phpMyAdmin: http://192.168.0.114/phpmyadmin/" -ForegroundColor White
Write-Host "- Direct database URL: http://192.168.0.114/phpmyadmin/index.php?route=/database/structure&db=hrms_database" -ForegroundColor White
Write-Host "- MySQL Workbench (if installed)" -ForegroundColor White
Write-Host ""
Write-Host "The application should now be ready to run!" -ForegroundColor Green
Write-Host "Use 'dotnet run' to start the application." -ForegroundColor Yellow
Write-Host ""
Read-Host "Press Enter to exit"