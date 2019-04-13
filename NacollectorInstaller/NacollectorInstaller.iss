#define MyAppName "Nacollector"
#define MyAppVersion "1.3.0.0"
#define MyAppPublisher "Copyright (C) 2019 qwqaq.com"
#define MyAppURL "https://qwqaq.com/"
#define MyAppExeName "Nacollector.exe"
#define MyAppExeBasePath "..\Nacollector\bin\x86\Nacollector\"
#define MyAppProjectBasePath "..\"
#define MyAppOutputPath "..\Nacollector\bin\x86\"

[Setup]
AppId=com.qwqaq.nacollector
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL="https://github.com/qwqcode/Nacollector"
AppUpdatesURL="https://github.com/qwqcode/Nacollector/releases"
DefaultDirName={userappdata}\{#MyAppName}
DisableProgramGroupPage=yes
LicenseFile="{#MyAppProjectBasePath}\LICENSE_ANSI"
OutputDir="{#MyAppOutputPath}"
OutputBaseFilename="{#MyAppName}Installer_v{#MyAppVersion}"
SetupIconFile="{#MyAppProjectBasePath}\Nacollector\Resources\setup_icons\SetupIcon.ico"
WizardSmallImageFile="{#MyAppProjectBasePath}\Nacollector\Resources\setup_icons\SetupWizardSmall.bmp"
WizardImageFile="{#MyAppProjectBasePath}\Nacollector\Resources\setup_icons\SetupWizard.bmp"
Compression=lzma2/max
DisableWelcomePage=no
SolidCompression=yes
PrivilegesRequired=lowest

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[LangOptions]
DialogFontName=Arial
DialogFontSize=8
WelcomeFontName=Arial
WelcomeFontSize=12
TitleFontName=Arial
TitleFontSize=29
CopyrightFontName=Arial
CopyrightFontSize=8

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}";

[Files]
Source: "{#MyAppExeBasePath}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{commonprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
procedure InitializeWizard();
var
  VersionLabel: TNewStaticText;
begin
  VersionLabel := TNewStaticText.Create(WizardForm);
  VersionLabel.Caption := Format('Version: %s', ['{#SetupSetting("AppVersion")}']);
  VersionLabel.Parent := WizardForm;
  VersionLabel.Left := ScaleX(16);
  VersionLabel.Top :=
    WizardForm.BackButton.Top +
    (WizardForm.BackButton.Height div 2) -
    (VersionLabel.Height div 2)
end;

// Open Browser
procedure OpenBrowser(Url: string);
var
  ErrorCode: Integer;
begin
  ShellExec('open', Url, '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
end;

function IsDotNetDetected(version: string; service: cardinal): boolean;
// Indicates whether the specified version and service pack of the .NET Framework is installed.
//
// version -- Specify one of these strings for the required .NET Framework version:
//    'v1.1'          .NET Framework 1.1
//    'v2.0'          .NET Framework 2.0
//    'v3.0'          .NET Framework 3.0
//    'v3.5'          .NET Framework 3.5
//    'v4\Client'     .NET Framework 4.0 Client Profile
//    'v4\Full'       .NET Framework 4.0 Full Installation
//    'v4.5'          .NET Framework 4.5
//    'v4.5.1'        .NET Framework 4.5.1
//    'v4.5.2'        .NET Framework 4.5.2
//    'v4.6'          .NET Framework 4.6
//    'v4.6.1'        .NET Framework 4.6.1
//    'v4.6.2'        .NET Framework 4.6.2
//    'v4.7'          .NET Framework 4.7
//
// service -- Specify any non-negative integer for the required service pack level:
//    0               No service packs required
//    1, 2, etc.      Service pack 1, 2, etc. required
var
    key, versionKey: string;
    install, release, serviceCount, versionRelease: cardinal;
    success: boolean;
begin
    versionKey := version;
    versionRelease := 0;

    // .NET 1.1 and 2.0 embed release number in version key
    if version = 'v1.1' then begin
        versionKey := 'v1.1.4322';
    end else if version = 'v2.0' then begin
        versionKey := 'v2.0.50727';
    end

    // .NET 4.5 and newer install as update to .NET 4.0 Full
    else if Pos('v4.', version) = 1 then begin
        versionKey := 'v4\Full';
        case version of
          'v4.5':   versionRelease := 378389;
          'v4.5.1': versionRelease := 378675; // 378758 on Windows 8 and older
          'v4.5.2': versionRelease := 379893;
          'v4.6':   versionRelease := 393295; // 393297 on Windows 8.1 and older
          'v4.6.1': versionRelease := 394254; // 394271 before Win10 November Update
          'v4.6.2': versionRelease := 394802; // 394806 before Win10 Anniversary Update
          'v4.7':   versionRelease := 460798; // 460805 before Win10 Creators Update
        end;
    end;

    // installation key group for all .NET versions
    key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + versionKey;

    // .NET 3.0 uses value InstallSuccess in subkey Setup
    if Pos('v3.0', version) = 1 then begin
        success := RegQueryDWordValue(HKLM, key + '\Setup', 'InstallSuccess', install);
    end else begin
        success := RegQueryDWordValue(HKLM, key, 'Install', install);
    end;

    // .NET 4.0 and newer use value Servicing instead of SP
    if Pos('v4', version) = 1 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Servicing', serviceCount);
    end else begin
        success := success and RegQueryDWordValue(HKLM, key, 'SP', serviceCount);
    end;

    // .NET 4.5 and newer use additional value Release
    if versionRelease > 0 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Release', release);
        success := success and (release >= versionRelease);
    end;

    result := success and (install = 1) and (serviceCount >= service);
end;


function InitializeSetup(): Boolean;
begin
    if not IsDotNetDetected('v4.6.2', 0) then begin
        SuppressibleMsgBox('当前 .NET Framework 版本过低'#13#13
        '请先升级 .NET Framework 至 4.6.2+ 后再运行安装程序', mbCriticalError, MB_OK, MB_OK);
        OpenBrowser('https://www.microsoft.com/zh-cn/download/details.aspx?id=53344');
        result := false;
    end else
        result := true;
end;