; DOCENTRA PDF & DOCUMENT EDITOR - Professional Installer Script
; Tool: Inno Setup (www.jrsoftware.org)

[Setup]
AppId={{D0C312A-1234-5678-90AB-CDEF12345678}}
AppName=DOCENTRA PDF & DOCUMENT EDITOR
AppVersion=1.0.2
AppPublisher=Docentra AI Team
AppPublisherURL=https://www.docentrapdf.com
AppSupportURL=https://www.docentrapdf.com
AppUpdatesURL=https://www.docentrapdf.com
DefaultDirName={autopf}\Docentra PDF Editor
DefaultGroupName=Docentra PDF Editor
AllowNoIcons=yes
UninstallDisplayIcon={app}\PromtAiPdfPro.exe
; İkon dosyası (.ico formatında olmalıdır, şimdilik .png üzerinden ikon ayarlanabilir ama gerçek installer için .ico tercih edilir)
SetupIconFile=PromtAiPdfPro\Assets\app_icon.ico
OutputDir=.
OutputBaseFilename=Docentra_Setup_v1.0.2
Compression=lzma
SolidCompression=yes
ShowLanguageDialog=auto
LanguageDetectionMethod=uilanguage

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"; LicenseFile: "License-english.txt"; InfoBeforeFile: "Requirements-english.txt"
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"; LicenseFile: "License-turkish.txt"; InfoBeforeFile: "Requirements-turkish.txt"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"; LicenseFile: "License-french.txt"; InfoBeforeFile: "Requirements-french.txt"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"; LicenseFile: "License-german.txt"; InfoBeforeFile: "Requirements-german.txt"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"; LicenseFile: "License-spanish.txt"; InfoBeforeFile: "Requirements-spanish.txt"
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"; LicenseFile: "License-italian.txt"; InfoBeforeFile: "Requirements-italian.txt"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"; LicenseFile: "License-japanese.txt"; InfoBeforeFile: "Requirements-japanese.txt"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"; LicenseFile: "License-russian.txt"; InfoBeforeFile: "Requirements-russian.txt"
Name: "arabic"; MessagesFile: "compiler:Languages\Arabic.isl"; LicenseFile: "License-arabic.txt"; InfoBeforeFile: "Requirements-arabic.txt"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Zırhlanmış ve full-runtime içeren publish klasörünü paketliyoruz
Source: "PromtAiPdfPro\bin\Release\net8.0-windows10.0.17763.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Not: Önce 'dotnet publish' yapılmış olmalıdır.

[Icons]
Name: "{group}\DOCENTRA PDF & DOCUMENT EDITOR"; Filename: "{app}\PromtAiPdfPro.exe"
Name: "{group}\{cm:UninstallProgram,DOCENTRA PDF & DOCUMENT EDITOR}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\DOCENTRA PDF & DOCUMENT EDITOR"; Filename: "{app}\PromtAiPdfPro.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\PromtAiPdfPro.exe"; Description: "{cm:LaunchProgram,DOCENTRA PDF & DOCUMENT EDITOR}"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup(): Boolean;
var
  ErrorCode: Integer;
begin
  // Basit bir .NET 8 kontrolü (Gelişmiş kontrol için Registry bakılabilir)
  // Şimdilik kullanıcıya uyarı veren veya direkt kuruluma izin veren bir yapı.
  Result := True;
end;
