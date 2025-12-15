[Setup]
AppName=Gamma Manager
AppVersion=1.2.0.0
DefaultDirName={autopf}\Gamma Manager
OutputBaseFilename=GammaSetup
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
DisableProgramGroupPage=yes
DisableDirPage=no

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Point this to your Release build folder
Source: "Gamma Manager\bin\Release\x86\Gamma Manager.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "Gamma Manager\bin\Release\x86\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\Gamma Manager"; Filename: "{app}\Gamma Manager.exe"
Name: "{autodesktop}\Gamma Manager"; Filename: "{app}\Gamma Manager.exe"; Tasks: desktopicon
