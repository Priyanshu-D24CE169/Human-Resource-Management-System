# HRMS Database Setup Script
# Run this script as Administrator

Write-Host "=== HRMS Database Setup Script ===" -ForegroundColor Green
Write-Host ""

# Function to check if running as administrator
function Test-Administrator {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# Check if running as administrator
if (-not (Test-Administrator)) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "Step 1: Checking SQL Server Express installation..." -ForegroundColor Yellow

# Check if SQL Server Express is installed
$sqlService = Get-Service -Name "MSSQL`$SQLEXPRESS" -ErrorAction SilentlyContinue
if ($sqlService) {
    Write-Host "? SQL Server Express found" -ForegroundColor Green
    
    # Start the service if it's not running
    if ($sqlService.Status -ne "Running") {
        Write-Host "Starting SQL Server Express service..." -ForegroundColor Yellow
        Start-Service "MSSQL`$SQLEXPRESS"
        Write-Host "? SQL Server Express started" -ForegroundColor Green
    } else {
        Write-Host "? SQL Server Express is running" -ForegroundColor Green
    }
} else {
    Write-Host "? SQL Server Express not found" -ForegroundColor Red
    Write-Host "Please install SQL Server Express from:" -ForegroundColor Yellow
    Write-Host "https://www.microsoft.com/en-us/sql-server/sql-server-downloads" -ForegroundColor Cyan
    Read-Host "Press Enter after installing SQL Server Express"
}

Write-Host ""
Write-Host "Step 2: Configuring Windows Firewall..." -ForegroundColor Yellow

try {
    # Configure firewall for SQL Server
    New-NetFirewallRule -DisplayName "SQL Server Express" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 1433 -ErrorAction SilentlyContinue
    New-NetFirewallRule -DisplayName "SQL Browser" -Direction Inbound -Action Allow -Protocol UDP -LocalPort 1434 -ErrorAction SilentlyContinue
    
    # Configure firewall for HRMS application
    New-NetFirewallRule -DisplayName "HRMS HTTPS" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 7037 -ErrorAction SilentlyContinue
    New-NetFirewallRule -DisplayName "HRMS HTTP" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 5194 -ErrorAction SilentlyContinue
    
    Write-Host "? Firewall rules configured" -ForegroundColor Green
} catch {
    Write-Host "? Firewall configuration may have failed" -ForegroundColor Yellow
    Write-Host "You may need to configure firewall manually" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Step 3: Getting network information..." -ForegroundColor Yellow

# Get local IP address
$localIP = (Get-NetIPAddress -AddressFamily IPv4 -InterfaceAlias "Ethernet*" | Select-Object -First 1).IPAddress
if (-not $localIP) {
    $localIP = (Get-NetIPAddress -AddressFamily IPv4 -InterfaceAlias "Wi-Fi*" | Select-Object -First 1).IPAddress
}

if ($localIP) {
    Write-Host "? Local IP Address: $localIP" -ForegroundColor Green
} else {
    Write-Host "? Could not determine local IP address" -ForegroundColor Yellow
    $localIP = "localhost"
}

Write-Host ""
Write-Host "Step 4: Setting up .NET environment..." -ForegroundColor Yellow

# Navigate to project directory
$projectPath = Split-Path -Parent $PSScriptRoot
Set-Location $projectPath

# Restore NuGet packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore

if ($LASTEXITCODE -eq 0) {
    Write-Host "? NuGet packages restored" -ForegroundColor Green
} else {
    Write-Host "? NuGet restore may have failed" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Step 5: Building application..." -ForegroundColor Yellow

# Build the application
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Application built successfully" -ForegroundColor Green
} else {
    Write-Host "? Build failed" -ForegroundColor Red
    Read-Host "Press Enter to continue anyway"
}

Write-Host ""
Write-Host "=== Setup Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Network Access Information:" -ForegroundColor Cyan
Write-Host "  Local Access:   https://localhost:7037" -ForegroundColor White
Write-Host "  Network Access: https://$localIP:7037" -ForegroundColor White
Write-Host ""
Write-Host "Default Login Credentials:" -ForegroundColor Cyan
Write-Host "  Email:    admin@hrms.com" -ForegroundColor White
Write-Host "  Password: admin123" -ForegroundColor White
Write-Host ""
Write-Host "To start the application, run:" -ForegroundColor Yellow
Write-Host "  dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "The database will be created automatically on first run." -ForegroundColor Green
Write-Host ""

# Ask if user wants to start the application now
$startNow = Read-Host "Do you want to start the application now? (y/n)"
if ($startNow -eq 'y' -or $startNow -eq 'Y') {
    Write-Host ""
    Write-Host "Starting HRMS application..." -ForegroundColor Green
    Write-Host "Press Ctrl+C to stop the application" -ForegroundColor Yellow
    Write-Host ""
    dotnet run
}