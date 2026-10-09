@echo off
setlocal
chcp 65001 >nul
title DOCENTRA PDF - Premium Lisans Anahtarı Üreteci

set "ROOT_DIR=%~dp0"
set "KEYGEN_EXE=%ROOT_DIR%tools\PremiumKeyGenerator.exe"

if exist "%KEYGEN_EXE%" (
    echo Premium Key Generator başlatılıyor...
    start "" "%KEYGEN_EXE%"
    exit /b 0
)

echo [UYARI] tools\PremiumKeyGenerator.exe bulunamadı!
pause
