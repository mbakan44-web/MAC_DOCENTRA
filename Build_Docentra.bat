@echo off
echo "Building DOCENTRA PDF & DOCUMENT EDITOR (Portable EXE)..."

REM Taşınabilir SDK yolunu belirle (Aynı dizindeki dotnet_sdk klasörü)
set "DOTNET_SDK=%~dp0dotnet_sdk\dotnet.exe"

cd PromtAiPdfPro
"%DOTNET_SDK%" publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishReadyToRun=true /p:IncludeNativeLibrariesForSelfExtract=true > build_log.txt 2>&1
if %errorlevel% neq 0 (
    echo "Build failed! Check build_log.txt"
    pause
    exit /b
)
echo.
echo "Build Complete!" 
echo "Your portable .exe is located in: PromtAiPdfPro\bin\Release\net8.0-windows10.0.17763.0\win-x64\publish\"
pause
