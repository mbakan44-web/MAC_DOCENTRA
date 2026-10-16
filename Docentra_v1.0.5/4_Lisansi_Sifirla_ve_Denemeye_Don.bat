@echo off
chcp 65001 >nul
title Docentra - Lisans Sıfırlama Aracı
color 0E

echo =======================================================================
echo   DOCENTRA PDF - LİSANS SIFIRLAMA VE DENEME SÜRÜMÜNE DÖNME ARACI
echo =======================================================================
echo.
echo Bu işlem mevcut kayıtlı lisans anahtarını siler ve programı
echo tekrar deneme / ücretsiz sürüm durumuna döndürür.
echo.
pause

echo Çalışan Docentra işlemleri kapatılıyor...
taskkill /F /IM Docentra.exe /T >nul 2>&1

echo Kayıt Defterindeki lisans anahtarı temizleniyor...
reg delete "HKCU\SOFTWARE\DocentraApp" /v "LicenseKey" /f >nul 2>&1

color 0A
echo.
echo =======================================================================
echo   [BAŞARILI] Lisans anahtarı başarıyla sıfırlandı!
echo   Docentra'yı açtığınızda artık Premium durumu pasif olacaktır.
echo =======================================================================
echo.
pause
