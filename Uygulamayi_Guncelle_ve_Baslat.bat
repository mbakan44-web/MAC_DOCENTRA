@echo off
REM Uygulamayi Guncelle ve Baslat
setlocal

set "ROOT=%~dp0"
set "DOTNET_SDK=%ROOT%dotnet_sdk\dotnet.exe"
set "DOTNET_RUN=%ROOT%dotnet_portable\dotnet.exe"
set "PROJECT=%ROOT%PromtAiPdfPro\PromtAiPdfPro.csproj"
set "DLL=%ROOT%PromtAiPdfPro\bin\Debug\net8.0-windows10.0.17763.0\PromtAiPdfPro.dll"

echo [1/2] Temizleniyor ve Guncelleniyor...
echo Lutfen bekleyin...

REM Süreçleri temizle (Maksimum Agresif)
echo Surecler temizleniyor...
taskkill /F /IM PromtAiPdfPro.exe /T 2>nul
taskkill /F /IM dotnet.exe /T 2>nul
wmic process where "name='PromtAiPdfPro.exe'" delete >nul 2>&1
wmic process where "name='dotnet.exe'" delete >nul 2>&1
timeout /t 2 /nobreak >nul

echo Temizleniyor (Clean)...
"%DOTNET_SDK%" clean "%PROJECT%" -c Debug

echo Derleniyor (Build)...
"%DOTNET_SDK%" build "%PROJECT%" -c Debug
if %errorlevel% neq 0 (
    echo Hata olustu.
    pause
    exit /b
)

echo.
echo [2/2] Baslatiliyor...
if exist "%DLL%" (
    start "" "%DOTNET_RUN%" "%DLL%"
) else (
    echo DLL bulunamadi: %DLL%
    pause
)

exit
