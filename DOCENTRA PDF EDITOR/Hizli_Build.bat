@echo off
setlocal
echo ======================================================
echo   DOCENTRA PDF - HIZLI STANDALONE BUILD (V3)
echo ======================================================
echo.

set "BASE_DIR=%~dp0"
set "DOTNET_SDK=%BASE_DIR%dotnet_sdk\dotnet.exe"
set "PROJECT_DIR=%BASE_DIR%PromtAiPdfPro"
set "STANDALONE_DIR=%PROJECT_DIR%\bin\Release\Standalone"

if not exist "%DOTNET_SDK%" (
    echo [HATA] dotnet_sdk bulunamadi! Lutfen SDK'nin dogru yerde oldugundan emin olun.
    pause
    exit /b
)

echo [1/4] Mevcut surecler sonlandiriliyor...
taskkill /F /IM Docentra.exe /T >nul 2>&1
timeout /t 1 /nobreak >nul

echo [2/4] Eski derleme artiklari temizleniyor...
if exist "%PROJECT_DIR%\bin" rd /s /q "%PROJECT_DIR%\bin"
if exist "%PROJECT_DIR%\obj" rd /s /q "%PROJECT_DIR%\obj"
del /q "%STANDALONE_DIR%\*_old*" >nul 2>&1

echo [3/4] Proje derleniyor (Bu biraz zaman alabilir)...
cd /d "%PROJECT_DIR%"
"%DOTNET_SDK%" publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishReadyToRun=true /p:IncludeNativeLibrariesForSelfExtract=true

if %errorlevel% neq 0 (
    echo.
    echo [HATA] Derleme sirasinda bir hata olustu!
    pause
    exit /b
)

echo [4/4] Standalone EXE kopyalaniyor...
if not exist "%STANDALONE_DIR%" mkdir "%STANDALONE_DIR%"

set "BUILD_OUTPUT=%PROJECT_DIR%\bin\Release\net8.0-windows10.0.17763.0\win-x64\publish"

if exist "%BUILD_OUTPUT%" (
    REM Dosyalari kopyala. Eger kilitliyse .old olarak yeniden adlandirip dene.
    xcopy /y /e "%BUILD_OUTPUT%\*" "%STANDALONE_DIR%\" >nul 2>&1
    if %errorlevel% neq 0 (
        echo [UYARI] Bazı dosyalar kilitli, yeniden adlandiriliyor...
        if exist "%STANDALONE_DIR%\Docentra.exe" ren "%STANDALONE_DIR%\Docentra.exe" "Docentra_old_%RANDOM%.exe"
        if exist "%STANDALONE_DIR%\pdfium.dll" ren "%STANDALONE_DIR%\pdfium.dll" "pdfium_old_%RANDOM%.dll"
        xcopy /y /e "%BUILD_OUTPUT%\*" "%STANDALONE_DIR%\"
    )
    
    echo.
    echo ======================================================
    echo   DERLEME BASARILI!
    echo   Yeni dosyalar: %STANDALONE_DIR%
    echo ======================================================
) else (
    echo [HATA] Derleme cikti klasoru bulunamadi.
)

echo.
echo Temizlik yapiliyor...
del /q "%STANDALONE_DIR%\*_old*" >nul 2>&1

echo.
pause
