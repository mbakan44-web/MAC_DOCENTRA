@echo off
title Docentra Mac - Windows Test Runner
echo --------------------------------------------------
echo Docentra Mac Uygulamasi Windows Uzerinde Baslatiliyor...
echo SDK Yolu: ..\dotnet_sdk\dotnet.exe (Goreceli Yol)
echo --------------------------------------------------

:: Bulundugu klasore odaklan (Docentra_Mac)
cd /d "%~dp0"

:: SDK'yi kullanarak projeyi calistir
"..\dotnet_sdk\dotnet.exe" run

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [Hata] Uygulama baslatilamadi! Lutfen yukaridaki hata mesajlarini kontrol edin.
    pause
)
