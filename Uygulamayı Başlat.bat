@echo off
set "RUNTIME_PATH=%~dp0dotnet_portable\dotnet.exe"
set "APP_PATH=%~dp0PromtAiPdfPro\bin\Debug\net8.0-windows10.0.17763.0\PromtAiPdfPro.dll"

if not exist "%RUNTIME_PATH%" (
    echo Hata: .NET Runtime dosyasi eksik.
    pause
    exit /b
)

if not exist "%APP_PATH%" (
    echo Hata: Uygulama DLL dosyasi bulunamadi.
    pause
    exit /b
)

echo Uygulama baslatiliyor...
"%RUNTIME_PATH%" "%APP_PATH%"
if %errorlevel% neq 0 (
    echo.
    echo Uygulama bir hata ile kapandi. Hata kodu: %errorlevel%
    pause
)
