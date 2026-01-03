# HRMS Quick Start Script
# Run this script to set up the database and run the application

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  HRMS - HR Management System Setup" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Check if dotnet is installed
Write-Host "Checking .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "? .NET SDK $dotnetVersion found" -ForegroundColor Green
}
catch {
    Write-Host "? .NET SDK not found. Please install .NET SDK first." -ForegroundColor Red
    exit 1
}

# Check if dotnet-ef is installed
Write-Host "`nChecking Entity Framework tools..." -ForegroundColor Yellow
try {
    $efVersion = dotnet ef --version
    Write-Host "? Entity Framework tools found" -ForegroundColor Green
}
catch {
    Write-Host "? Entity Framework tools not found. Installing..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    Write-Host "? Entity Framework tools installed" -ForegroundColor Green
}

# Restore NuGet packages
Write-Host "`nRestoring NuGet packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -eq 0) {
    Write-Host "? NuGet packages restored successfully" -ForegroundColor Green
} else {
    Write-Host "? Failed to restore packages" -ForegroundColor Red
    exit 1
}

# Create database migration
Write-Host "`nCreating database migration..." -ForegroundColor Yellow
if (Test-Path "Migrations") {
    Write-Host "Migrations folder already exists. Skipping..." -ForegroundColor Yellow
} else {
    dotnet ef migrations add InitialCreate
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Migration created successfully" -ForegroundColor Green
    } else {
        Write-Host "? Failed to create migration" -ForegroundColor Red
        exit 1
    }
}

# Update database
Write-Host "`nUpdating database..." -ForegroundColor Yellow
dotnet ef database update
if ($LASTEXITCODE -eq 0) {
    Write-Host "? Database updated successfully" -ForegroundColor Green
} else {
    Write-Host "? Failed to update database" -ForegroundColor Red
    Write-Host "Please check your connection string in appsettings.json" -ForegroundColor Yellow
    exit 1
}

# Create uploads directory
Write-Host "`nCreating uploads directory..." -ForegroundColor Yellow
$uploadsPath = "wwwroot/uploads"
if (-not (Test-Path $uploadsPath)) {
    New-Item -ItemType Directory -Path "$uploadsPath/profiles" -Force | Out-Null
    New-Item -ItemType Directory -Path "$uploadsPath/resumes" -Force | Out-Null
    New-Item -ItemType Directory -Path "$uploadsPath/leave-attachments" -Force | Out-Null
    Write-Host "? Uploads directory created" -ForegroundColor Green
} else {
    Write-Host "? Uploads directory already exists" -ForegroundColor Green
}

Write-Host "`n================================================" -ForegroundColor Cyan
Write-Host "  Setup Complete!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Default Admin Credentials:" -ForegroundColor Yellow
Write-Host "  Login ID: ADMIN001" -ForegroundColor White
Write-Host "  Password: Admin@123" -ForegroundColor White
Write-Host ""
Write-Host "To start the application, run:" -ForegroundColor Yellow
Write-Host "  dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "Or press F5 in Visual Studio" -ForegroundColor Yellow
Write-Host ""
Write-Host "Application will be available at:" -ForegroundColor Yellow
Write-Host "  https://localhost:5001" -ForegroundColor White
Write-Host "  http://localhost:5000" -ForegroundColor White
Write-Host ""

# Ask if user wants to run the application now
$run = Read-Host "Do you want to run the application now? (Y/N)"
if ($run -eq "Y" -or $run -eq "y") {
    Write-Host "`nStarting application..." -ForegroundColor Green
    dotnet run
} else {
    Write-Host "`nSetup complete. Run 'dotnet run' when ready." -ForegroundColor Green
}
