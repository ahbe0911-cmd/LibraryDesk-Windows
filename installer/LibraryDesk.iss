#define MyAppName "میزکار هوشمند کتابخانه"
#define MyAppVersion "2.0.0"
#define MyAppPublisher "LibraryDesk"
#define MyAppExeName "LibraryDesk.exe"

[Setup]
AppId={{A82A6E2D-7F68-4F20-93A8-6B1E7190A801}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\LibraryDesk
DefaultGroupName={#MyAppName}
OutputDir=..\installer-output
OutputBaseFilename=LibraryDesk-Setup-x64
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest

[Files]
Source: "..\publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "ایجاد میانبر روی دسکتاپ"; GroupDescription: "میانبرها:"; Flags: unchecked

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "اجرای میزکار هوشمند کتابخانه"; Flags: nowait postinstall skipifsilent
