#define AppName        GetStringFileInfo('..\Binaries\HamCheck.exe', 'ProductName')
#define AppVersion     GetStringFileInfo('..\Binaries\HamCheck.exe', 'ProductVersion')
#define AppFileVersion GetStringFileInfo('..\Binaries\HamCheck.exe', 'FileVersion')
#define AppCompany     GetStringFileInfo('..\Binaries\HamCheck.exe', 'CompanyName')
#define AppCopyright   GetStringFileInfo('..\Binaries\HamCheck.exe', 'LegalCopyright')
#define AppBase        LowerCase(StringChange(AppName, ' ', ''))
#define AppSetupFile   AppBase + StringChange(AppVersion, '.', '')

#define AppVersionEx   StringChange(AppVersion, '0.00', '')
#if "" != VersionHash
#  define AppVersionEx AppVersionEx + " (" + VersionHash + ")"
#endif


[Setup]
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppCompany}
AppPublisherURL=http://jmedved.com/{#AppBase}/
AppCopyright={#AppCopyright}
VersionInfoProductVersion={#AppVersion}
VersionInfoProductTextVersion={#AppVersionEx}
VersionInfoVersion={#AppFileVersion}
DefaultDirName={pf}\{#AppCompany}\{#AppName}
OutputBaseFilename={#AppSetupFile}
OutputDir=..\Releases
SourceDir=..\Binaries
AppId=JosipMedved_HamCheck
CloseApplications="yes"
RestartApplications="no"
UninstallDisplayIcon={app}\HamCheck.exe
AlwaysShowComponentsList=no
ArchitecturesInstallIn64BitMode=x64
DisableProgramGroupPage=yes
MergeDuplicateFiles=yes
MinVersion=0,5.1
PrivilegesRequired=admin
ShowLanguageDialog=no
SolidCompression=yes
ChangesAssociations=no
DisableWelcomePage=yes
LicenseFile=..\Setup\License.rtf


[Messages]
SetupAppTitle=Setup {#AppName} {#AppVersionEx}
SetupWindowTitle=Setup {#AppName} {#AppVersionEx}
BeveledLabel=jmedved.com

[Dirs]
Name: "{userappdata}\Josip Medved\HamCheck";  Flags: uninsalwaysuninstall

[Files]
Source: "HamCheck.exe";      DestDir: "{app}";                      Flags: ignoreversion;
Source: "HamCheck.pdb";      DestDir: "{app}";                      Flags: ignoreversion;
Source: "HamCheckExam.dll";  DestDir: "{app}";                      Flags: ignoreversion;
Source: "HamCheckExam.pdb";  DestDir: "{app}";                      Flags: ignoreversion;
Source: "ReadMe.txt";        DestDir: "{app}";  Attribs: readonly;  Flags: overwritereadonly uninsremovereadonly;
Source: "License.txt";       DestDir: "{app}";  Attribs: readonly;  Flags: overwritereadonly uninsremovereadonly;

[Icons]
Name: "{userstartmenu}\Ham Check";  Filename: "{app}\HamCheck.exe"

[Registry]
Root: HKCU;  Subkey: "Software\Josip Medved";                                                                                                                Flags: uninsdeletekeyifempty
Root: HKCU;  Subkey: "Software\Josip Medved\HamCheck";                                                                                                       Flags: uninsdeletekey

[Run]
Description: "Launch application now";  Filename: "{app}\HamCheck.exe";   Parameters: "/setup";  Flags: postinstall nowait skipifsilent runasoriginaluser shellexec
Description: "View ReadMe.txt";         Filename: "{app}\ReadMe.txt";                         Flags: postinstall nowait skipifsilent runasoriginaluser shellexec unchecked

[Code]

procedure InitializeWizard;
begin
  WizardForm.LicenseAcceptedRadio.Checked := True;
end;
