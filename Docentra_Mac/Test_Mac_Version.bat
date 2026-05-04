@echo off
title Docentra Mac Version Test Runner
echo [INFO] Starting Docentra Mac Version on Windows...
echo [INFO] Portable SDK is being used.

:: Navigate to project directory (the bat is already there but just in case)
cd /d "%~dp0"

:: Run the application and redirect output/errors to a temporary file
..\dotnet_sdk\dotnet.exe run --project Docentra_Mac.csproj > run_output.tmp 2>&1

:: Check if the application exited with an error
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Application crashed! Checking logs...
    echo %DATE% %TIME% - CRASH DETECTED >> crash_log.txt
    type run_output.tmp >> crash_log.txt
    echo ------------------------------------------ >> crash_log.txt
    echo [ALERT] A crash log has been created in crash_log.txt
    pause
) else (
    echo [SUCCESS] Application closed normally.
)

:: Clean up temporary file
if exist run_output.tmp del run_output.tmp
