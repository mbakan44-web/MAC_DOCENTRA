@echo off
setlocal
chcp 65001 >nul
title DOCENTRA PDF - v1.0.4 Başlatıcı

set "ROOT_DIR=%~dp0"
set "EXE_DIST=%ROOT_DIR%dist\Docentra.exe"
set "EXE_PUBLISH=%ROOT_DIR%dist\publish\Docentra.exe"
set "EXE_SRC=%ROOT_DIR%src\bin\Release\net8.0-windows10.0.17763.0\win-x64\publish\Docentra.exe"

if exist "%EXE_DIST%" (
    echo Docentra v1.0.4 başlatılıyor...
    start "" "%EXE_DIST%"
    exit /b 0
)

if exist "%EXE_PUBLISH%" (
    echo Docentra v1.0.4 başlatılıyor...
    start "" "%EXE_PUBLISH%"
    exit /b 0
)

if exist "%EXE_SRC%" (
    echo Docentra v1.0.4 başlatılıyor...
    start "" "%EXE_SRC%"
    exit /b 0
)

echo [UYARI] Henüz derlenmiş bir Docentra.exe bulunamadı!
echo Lütfen önce "1_Hizli_Derle_ve_Paketle.bat" dosyasını çalıştırın.
pause
