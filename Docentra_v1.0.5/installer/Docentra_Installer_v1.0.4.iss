; DOCENTRA PDF & DOCUMENT EDITOR - Professional Installer Script
; Tool: Inno Setup (www.jrsoftware.org)

[Setup]
AppId={{D0C312A-1234-5678-90AB-CDEF12345678}}
AppName=Docentra PDF Suite (Viewer & Editor)
AppVersion=1.0.5
AppPublisher=Docentra AI Team
AppPublisherURL=https://www.docentrapdf.com
AppSupportURL=https://www.docentrapdf.com
AppUpdatesURL=https://www.docentrapdf.com
DefaultDirName={autopf}\Docentra PDF Editor
DefaultGroupName=Docentra PDF Editor
AllowNoIcons=yes
UninstallDisplayIcon={app}\Docentra.exe
; İkon dosyası (.ico formatında olmalıdır, şimdilik .png üzerinden ikon ayarlanabilir ama gerçek installer için .ico tercih edilir)
SetupIconFile=..\src\Assets\app_icon.ico
OutputDir=..\dist
OutputBaseFilename=Docentra_Setup_v1.0.5
Compression=lzma2/max
SolidCompression=yes
ShowLanguageDialog=auto
LanguageDetectionMethod=uilanguage
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"; LicenseFile: "languages\License-english.txt"; InfoBeforeFile: "languages\Requirements-english.txt"
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"; LicenseFile: "languages\License-turkish.txt"; InfoBeforeFile: "languages\Requirements-turkish.txt"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"; LicenseFile: "languages\License-french.txt"; InfoBeforeFile: "languages\Requirements-french.txt"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"; LicenseFile: "languages\License-german.txt"; InfoBeforeFile: "languages\Requirements-german.txt"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"; LicenseFile: "languages\License-spanish.txt"; InfoBeforeFile: "languages\Requirements-spanish.txt"
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"; LicenseFile: "languages\License-italian.txt"; InfoBeforeFile: "languages\Requirements-italian.txt"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"; LicenseFile: "languages\License-japanese.txt"; InfoBeforeFile: "languages\Requirements-japanese.txt"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"; LicenseFile: "languages\License-russian.txt"; InfoBeforeFile: "languages\Requirements-russian.txt"
Name: "arabic"; MessagesFile: "compiler:Languages\Arabic.isl"; LicenseFile: "languages\License-arabic.txt"; InfoBeforeFile: "languages\Requirements-arabic.txt"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Zırhlanmış ve full-runtime içeren publish klasörünü paketliyoruz
Source: "..\dist\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Not: Önce 'dotnet publish' yapılmış olmalıdır.

[Icons]
Name: "{group}\Docentra PDF Suite (Viewer & Editor)"; Filename: "{app}\Docentra.exe"
Name: "{group}\{cm:UninstallProgram,Docentra PDF Suite (Viewer & Editor)}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Docentra PDF Suite (Viewer & Editor)"; Filename: "{app}\Docentra.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\Docentra.exe"; Description: "{cm:LaunchProgram,Docentra PDF Suite (Viewer & Editor)}"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup(): Boolean;
var
  ErrorCode: Integer;
begin
  // Basit bir .NET 8 kontrolü (Gelişmiş kontrol için Registry bakılabilir)
  // Şimdilik kullanıcıya uyarı veren veya direkt kuruluma izin veren bir yapı.
  Result := True;
end;
