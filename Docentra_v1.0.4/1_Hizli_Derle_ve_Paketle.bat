@echo off
setlocal
chcp 65001 >nul
title DOCENTRA PDF - v1.0.4 Derleme ve Paketleme Sistemi
color 0B

echo =======================================================================
echo   DOCENTRA PDF SUITE v1.0.4 - TAM OTOMATİK DERLEME VE PAKETLEME
echo =======================================================================
echo.

set "ROOT_DIR=%~dp0"
set "DOTNET_SDK=%ROOT_DIR%dotnet_sdk\dotnet.exe"
set "SRC_DIR=%ROOT_DIR%src"
set "DIST_DIR=%ROOT_DIR%dist"
set "PUBLISH_DIR=%DIST_DIR%\publish"

if not exist "%DOTNET_SDK%" (
    color 0C
    echo [HATA] dotnet_sdk bulunamadı!
    echo Yol: %DOTNET_SDK%
    pause
    exit /b 1
)

echo [1/4] Çalışan Docentra işlemleri kapatılıyor...
taskkill /F /IM Docentra.exe /T >nul 2>&1
timeout /t 1 /nobreak >nul

echo [2/4] Eski geçici derleme dosyaları temizleniyor...
if exist "%SRC_DIR%\bin" rd /s /q "%SRC_DIR%\bin" >nul 2>&1
if exist "%SRC_DIR%\obj" rd /s /q "%SRC_DIR%\obj" >nul 2>&1
if not exist "%DIST_DIR%" mkdir "%DIST_DIR%"
if not exist "%PUBLISH_DIR%" mkdir "%PUBLISH_DIR%"

echo [3/4] Proje Release modunda derleniyor ve paketleniyor (Lütfen bekleyin)...
cd /d "%SRC_DIR%"
"%DOTNET_SDK%" publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishReadyToRun=true /p:IncludeNativeLibrariesForSelfExtract=true

if %errorlevel% neq 0 (
    color 0C
    echo.
    echo =======================================================================
    echo [HATA] Derleme başarısız oldu!
    echo =======================================================================
    pause
    exit /b 1
)

echo [4/4] Çıktılar dist\ klasörüne kopyalanıyor...
set "BUILD_OUTPUT=%SRC_DIR%\bin\Release\net8.0-windows10.0.17763.0\win-x64\publish"

if exist "%BUILD_OUTPUT%" (
    xcopy /y /e /q "%BUILD_OUTPUT%\*" "%PUBLISH_DIR%\" >nul
    if exist "%PUBLISH_DIR%\Docentra.exe" (
        copy /y "%PUBLISH_DIR%\Docentra.exe" "%DIST_DIR%\Docentra.exe" >nul
    )
    if exist "%PUBLISH_DIR%\pdfium.dll" (
        copy /y "%PUBLISH_DIR%\pdfium.dll" "%DIST_DIR%\pdfium.dll" >nul
    )
)

color 0A
echo.
echo =======================================================================
echo   [BAŞARILI] v1.0.4 DERLEME TAMAMLANDI!
echo   * Çalıştırılabilir Tek Dosya EXE: dist\Docentra.exe
echo   * Inno Setup İçin Paket:         dist\publish\
echo =======================================================================
echo.
pause
