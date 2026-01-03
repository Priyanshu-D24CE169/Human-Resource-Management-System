# Salary Feature Fix Script
# Run this script to diagnose and fix salary-related issues

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  HRMS - Salary Feature Fix Tool" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Function to check if a table exists
function Test-TableExists {
    param([string]$connectionString, [string]$tableName)
    
    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $connection.Open()
        
        $query = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '$tableName'"
        $command = New-Object System.Data.SqlClient.SqlCommand($query, $connection)
        $result = $command.ExecuteScalar()
        
        $connection.Close()
        return $result -gt 0
    }
    catch {
        Write-Host "Error checking table existence: $_" -ForegroundColor Red
        return $false
    }
}

# Step 1: Check database connection
Write-Host "Step 1: Checking database connection..." -ForegroundColor Yellow
$connectionString = "Server=(localdb)\mssqllocaldb;Database=HRMS_DB;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    $connection.Close()
    Write-Host "? Database connection successful" -ForegroundColor Green
}
catch {
    Write-Host "? Database connection failed" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please ensure SQL Server LocalDB is running" -ForegroundColor Yellow
    exit 1
}

# Step 2: Check if Salaries table exists
Write-Host "`nStep 2: Checking if Salaries table exists..." -ForegroundColor Yellow
$salariesExists = Test-TableExists -connectionString $connectionString -tableName "Salaries"

if ($salariesExists) {
    Write-Host "? Salaries table exists" -ForegroundColor Green
    
    # Check if there are any salary records
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    $query = "SELECT COUNT(*) FROM Salaries"
    $command = New-Object System.Data.SqlClient.SqlCommand($query, $connection)
    $count = $command.ExecuteScalar()
    $connection.Close()
    
    Write-Host "  Found $count salary record(s)" -ForegroundColor Cyan
    
    if ($count -eq 0) {
        Write-Host "  ? No salary records found" -ForegroundColor Yellow
        Write-Host "  Run the CreateSalariesForEmployees.sql script to add default salaries" -ForegroundColor Yellow
    }
}
else {
    Write-Host "? Salaries table does not exist" -ForegroundColor Red
    Write-Host "  The table needs to be created via migration" -ForegroundColor Yellow
}

# Step 3: Check migrations
Write-Host "`nStep 3: Checking migrations..." -ForegroundColor Yellow
if (Test-Path "Migrations") {
    $migrations = Get-ChildItem -Path "Migrations" -Filter "*.cs" | Where-Object { $_.Name -notlike "*Designer.cs" }
    Write-Host "? Found $($migrations.Count) migration(s)" -ForegroundColor Green
    
    # Check if any migration includes Salaries
    $hasSalaryMigration = $false
    foreach ($migration in $migrations) {
        $content = Get-Content $migration.FullName -Raw
        if ($content -match "Salaries") {
            $hasSalaryMigration = $true
            Write-Host "  ? Migration includes Salaries table: $($migration.Name)" -ForegroundColor Green
        }
    }
    
    if (-not $hasSalaryMigration) {
        Write-Host "  ? No migration found that creates Salaries table" -ForegroundColor Yellow
    }
}
else {
    Write-Host "? Migrations folder not found" -ForegroundColor Red
}

# Step 4: Recommendations
Write-Host "`n================================================" -ForegroundColor Cyan
Write-Host "  Recommendations:" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

if (-not $salariesExists) {
    Write-Host ""
    Write-Host "ISSUE: Salaries table is missing" -ForegroundColor Red
    Write-Host ""
    Write-Host "SOLUTION: Create the table via migration" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Option 1: Add new migration (keeps existing data)" -ForegroundColor Cyan
    Write-Host "  dotnet ef migrations add AddSalaryTable" -ForegroundColor White
    Write-Host "  dotnet ef database update" -ForegroundColor White
    Write-Host ""
    Write-Host "Option 2: Recreate database (WARNING: deletes all data)" -ForegroundColor Cyan
    Write-Host "  dotnet ef database drop -f" -ForegroundColor White
    Write-Host "  Remove-Item Migrations -Recurse" -ForegroundColor White
    Write-Host "  dotnet ef migrations add InitialCreate" -ForegroundColor White
    Write-Host "  dotnet ef database update" -ForegroundColor White
    Write-Host ""
}
elseif ($count -eq 0) {
    Write-Host ""
    Write-Host "ISSUE: Salaries table exists but no salary records" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "SOLUTION: Add salary records for existing employees" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. Open SQL Server Management Studio or Azure Data Studio" -ForegroundColor Cyan
    Write-Host "2. Connect to: (localdb)\mssqllocaldb" -ForegroundColor White
    Write-Host "3. Open database: HRMS_DB" -ForegroundColor White
    Write-Host "4. Run the CreateSalariesForEmployees.sql script" -ForegroundColor White
    Write-Host ""
    Write-Host "OR recreate employees through the application" -ForegroundColor Cyan
    Write-Host ""
}
else {
    Write-Host ""
    Write-Host "? Salaries table exists with $count record(s)" -ForegroundColor Green
    Write-Host ""
    Write-Host "If you still can't see salary information:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. Ensure you're logged in as Admin" -ForegroundColor White
    Write-Host "2. Navigate to Employee ? Profile" -ForegroundColor White
    Write-Host "3. Look for 'Salary Info' tab" -ForegroundColor White
    Write-Host "4. If tab is missing, check that you have Admin role" -ForegroundColor White
    Write-Host ""
}

# Step 5: Offer to run migration
if (-not $salariesExists) {
    Write-Host "================================================" -ForegroundColor Cyan
    $response = Read-Host "`nWould you like to create a new migration now? (Y/N)"
    
    if ($response -eq "Y" -or $response -eq "y") {
        Write-Host "`nCreating migration..." -ForegroundColor Green
        dotnet ef migrations add AddSalaryFeature
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "? Migration created successfully" -ForegroundColor Green
            
            $updateResponse = Read-Host "`nWould you like to update the database now? (Y/N)"
            if ($updateResponse -eq "Y" -or $updateResponse -eq "y") {
                Write-Host "`nUpdating database..." -ForegroundColor Green
                dotnet ef database update
                
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "? Database updated successfully" -ForegroundColor Green
                    Write-Host ""
                    Write-Host "Salary table should now be available!" -ForegroundColor Green
                    Write-Host "Restart your application and check again." -ForegroundColor Yellow
                }
                else {
                    Write-Host "? Database update failed" -ForegroundColor Red
                }
            }
        }
        else {
            Write-Host "? Migration creation failed" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "Script completed." -ForegroundColor Cyan
Write-Host ""
