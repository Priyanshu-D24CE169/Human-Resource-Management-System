@echo off
echo === HRMS Quick Start ===
echo.

REM Check if running as administrator
net session >nul 2>&1
if %errorLevel% == 0 (
    echo Running with Administrator privileges...
    powershell.exe -ExecutionPolicy Bypass -File "%~dp0Setup-Database.ps1"
) else (
    echo This script needs to be run as Administrator.
    echo Right-click this file and select "Run as administrator"
    echo.
    echo Alternatively, you can run the application directly with:
    echo   dotnet run
    echo.
    pause
)