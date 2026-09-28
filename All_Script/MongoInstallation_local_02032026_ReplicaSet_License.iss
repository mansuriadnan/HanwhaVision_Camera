; MongoDB .NET Installation Script Installer
; Local files only version with existing installation detection
; REFACTORED VERSION - Using #define constants for maintainability

; ============================================================================
; GLOBAL CONSTANTS - Define all repeated values here
; ============================================================================

; Application Information
#define AppName "Vision Insight Web Application"
#define AppVersion "4.0.0"
#define AppShortName "Vision Insight"
#define AppInstallDir "Vision Insight Installer"
#define AppIconFile "Group_1.ico"
#define AppURL "http://localhost:8090"

; Dependencies Folder
#define DepsFolder "Dependencies"

; .NET Versions and Files
#define DotNetHostingVersion "8.0.23"
#define DotNetHostingFile "dotnet-hosting-8.0.23-win.exe"
#define DotNetSDKVersion "8.0.23"
#define DotNetSDKFile "aspnetcore-runtime-8.0.23-win-x64.exe"

; MongoDB Versions and Files
#define MongoDBVersion "7.0.12"
#define MongoDBFile "mongodb-windows-x86_64-7.0.12-signed.msi"
#define MongoShellVersion "2.3.0"
#define MongoShellFile "mongosh-2.3.0-x64.msi"
#define MongoToolsVersion "100.11.0"
#define MongoToolsFile "mongodb-database-tools-windows-x86_64-100.11.0.msi"

; MongoDB Configuration
#define MongoPort "27017"
#define MongoBindIP "127.0.0.1"
#define MongoReplicaSetName "rs0"
#define MongoLocalhost "localhost:27017"
#define MongoEnvVarName "VisionInsightMongoConn"
#define MongoDefaultConnString "mongodb://localhost:27017"
#define MongoDBName "visioninsightBIDashboard"

; Website 
#define SiteName "MyApp"
#define AppPoolName "MyAppPool"
#define DefaultSiteName "Default Web Site"
#define SitePort "8090"

; PowerShell Commands
#define PSExecutionPolicy "-NoProfile -ExecutionPolicy Bypass"

; Error Messages
#define ErrorMissingFile "Required file missing: "
#define ErrorEnsureDeps "Please ensure all dependency files are in the Dependencies folder."
#define ErrorPowerShell "PowerShell is required but not available on this system."

; Success Messages
;#define MsgReplicaSetManual "You can manually run this command in mongosh:"
;#define MsgReplicaSetCommand "rs.initiate({ _id: ""rs0"", members: [{ _id: 0, host: ""localhost:27017"" }] })"

[Setup]
AppName={#AppName}
AppVersion={#AppVersion}
DefaultDirName={autopf}\{#AppInstallDir}
DefaultGroupName={#AppInstallDir}
OutputDir=Output
OutputBaseFilename={#AppInstallDir}
PrivilegesRequired=admin
ArchitecturesAllowed=x64os
ArchitecturesInstallIn64BitMode=x64os
PrivilegesRequiredOverridesAllowed=dialog
SetupIconFile={#AppIconFile}
UninstallDisplayIcon={app}\{#AppIconFile}
ChangesEnvironment=yes


[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Icons]
; Start Menu shortcut
Name: "{group}\{#AppShortName}"; Filename: "{#AppURL}"; IconFilename: "{app}\{#AppIconFile}"

; Desktop shortcut (optional)
Name: "{commondesktop}\{#AppShortName}"; Filename: "{#AppURL}"; IconFilename: "{app}\{#AppIconFile}"

[Files]

Source: "{#AppIconFile}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{src}\{#DepsFolder}\{#DotNetHostingFile}"; DestDir: "{tmp}"; Flags: external
Source: "{src}\{#DepsFolder}\{#DotNetSDKFile}"; DestDir: "{tmp}"; Flags: external
Source: "{src}\{#DepsFolder}\{#MongoDBFile}"; DestDir: "{tmp}"; Flags: external
Source: "{src}\{#DepsFolder}\{#MongoShellFile}"; DestDir: "{tmp}"; Flags: external
Source: "{src}\{#DepsFolder}\{#MongoToolsFile}"; DestDir: "{tmp}"; Flags: external
Source: "{#DepsFolder}\testrunningapp.ps1"; DestDir: "{app}\{#DepsFolder}"; Flags: ignoreversion

[Code]
var
  ComponentsPage: TWizardPage;
  IISSetupCheckBox: TCheckBox;
  NetHostingCheckBox: TCheckBox;
  InstallDotNetSDKCheckBox:TCheckBox;
  MongoDBCheckBox: TCheckBox;
  MongoToolsCheckBox: TCheckBox;
  CreateShortcutCheckBox: TCheckBox;
  RestartCheckBox: TCheckBox;
  ProgressPage: TOutputProgressWizardPage;
  ResultCode: Integer;
  InstallSuccessful: Boolean;
  
  // New variables for MongoDB configuration
  MongoConfigPage: TWizardPage;
  RadioDefault: TRadioButton;
  RadioCustomPath: TRadioButton;
  RadioConnectionString: TRadioButton;
  
  LabelCustomPath: TLabel;
  EditCustomPath: TEdit;
  BtnBrowsePath: TButton;
  
  LabelConnectionString: TLabel;
  EditConnectionString: TEdit;
  
  SelectedMongoOption: Integer; // 1=Default, 2=Custom Path, 3=Connection String
  CustomMongoPath: String;
  MongoConnectionString: String;

function InitializeSetup: Boolean;
var
  ResultCode: Integer;
  SetupDir: String;
  DebugMsg: String;
  DotNetHostingPath: String;
  DotNetSDKPath: String;
  MongoDBPath: String;
  MongoShellPath: String;
  MongoToolsPath: String;
begin
  Result := True;
  
  // Check if PowerShell is available
  if not Exec('powershell.exe', '-Command "Write-Host PowerShell OK"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) or (ResultCode <> 0) then
  begin
    MsgBox('{#ErrorPowerShell}', mbError, MB_OK);
    Result := False;
    Exit;
  end;
  
  // Get the setup directory and show debug info
  SetupDir := ExtractFilePath(ExpandConstant('{srcexe}'));
  DebugMsg := 'Setup Directory: ' + SetupDir + #13#10 + #13#10;
  
  // Build full paths using constants
  DotNetHostingPath := SetupDir + '{#DepsFolder}\{#DotNetHostingFile}';
  DotNetSDKPath := SetupDir + '{#DepsFolder}\{#DotNetSDKFile}';
  MongoDBPath := SetupDir + '{#DepsFolder}\{#MongoDBFile}';
  MongoShellPath := SetupDir + '{#DepsFolder}\{#MongoShellFile}';
  MongoToolsPath := SetupDir + '{#DepsFolder}\{#MongoToolsFile}';
  
  // Check if Dependencies folder exists
  if DirExists(SetupDir + '{#DepsFolder}') then
    DebugMsg := DebugMsg + '{#DepsFolder} folder: EXISTS' + #13#10
  else
    DebugMsg := DebugMsg + '{#DepsFolder} folder: NOT FOUND' + #13#10;
  
  // Check each required file and show full paths
  DebugMsg := DebugMsg + #13#10 + 'File checks:' + #13#10;
  
  if FileExists(DotNetHostingPath) then
    DebugMsg := DebugMsg + '✓ {#DotNetHostingFile}: FOUND' + #13#10
  else begin
    DebugMsg := DebugMsg + '✗ {#DotNetHostingFile}: NOT FOUND' + #13#10;
    DebugMsg := DebugMsg + '  Expected at: ' + DotNetHostingPath + #13#10;
  end;
  
  if FileExists(DotNetSDKPath) then
    DebugMsg := DebugMsg + '✓ {#DotNetSDKFile}: FOUND' + #13#10
  else begin
    DebugMsg := DebugMsg + '✗ {#DotNetSDKFile}: NOT FOUND' + #13#10;
    DebugMsg := DebugMsg + '  Expected at: ' + DotNetSDKPath + #13#10;
  end;
  
  if FileExists(MongoDBPath) then
    DebugMsg := DebugMsg + '✓ {#MongoDBFile}: FOUND' + #13#10
  else begin
    DebugMsg := DebugMsg + '✗ {#MongoDBFile}: NOT FOUND' + #13#10;
    DebugMsg := DebugMsg + '  Expected at: ' + MongoDBPath + #13#10;
  end;
  
  if FileExists(MongoShellPath) then
    DebugMsg := DebugMsg + '✓ {#MongoShellFile}: FOUND' + #13#10
  else begin
    DebugMsg := DebugMsg + '✗ {#MongoShellFile}: NOT FOUND' + #13#10;
    DebugMsg := DebugMsg + '  Expected at: ' + MongoShellPath + #13#10;
  end;
  
   if FileExists(MongoToolsPath) then
    DebugMsg := DebugMsg + '✓ {#MongoToolsFile}: FOUND' + #13#10
  else begin
    DebugMsg := DebugMsg + '✗ {#MongoToolsFile}: NOT FOUND' + #13#10;
    DebugMsg := DebugMsg + '  Expected at: ' + MongoToolsPath + #13#10;
  end;
  
  // Show debug information
  //MsgBox(DebugMsg, mbInformation, MB_OK);
  
  // Now check if all required dependency files exist
  
  if not FileExists(DotNetHostingPath) then
  begin
    MsgBox('{#ErrorMissingFile}{#DepsFolder}\{#DotNetHostingFile}' + #13#10 + 
           '{#ErrorEnsureDeps}', mbError, MB_OK);
    Result := False;
  end;
  
  if not FileExists(DotNetSDKPath) then
  begin
    MsgBox('{#ErrorMissingFile}{#DepsFolder}\{#DotNetSDKFile}' + #13#10 + 
           '{#ErrorEnsureDeps}', mbError, MB_OK);
    Result := False;
  end;
  
  if not FileExists(MongoDBPath) then
  begin
    MsgBox('{#ErrorMissingFile}{#DepsFolder}\{#MongoDBFile}' + #13#10 + 
           '{#ErrorEnsureDeps}', mbError, MB_OK);
    Result := False;
  end;
  
  if not FileExists(MongoShellPath) then
  begin
    MsgBox('{#ErrorMissingFile}{#DepsFolder}\{#MongoShellFile}' + #13#10 + 
           '{#ErrorEnsureDeps}', mbError, MB_OK);
    Result := False;
  end;
  
  if not FileExists(MongoToolsPath) then
  begin
    MsgBox('{#ErrorMissingFile}{#DepsFolder}\{#MongoToolsFile}' + #13#10 + 
           '{#ErrorEnsureDeps}', mbError, MB_OK);
    Result := False;
  end;
end;

// Helper function to show/hide controls based on radio button selection
procedure UpdateMongoControlsVisibility;
begin
  // Hide all optional controls first
  LabelCustomPath.Visible := False;
  EditCustomPath.Visible := False;
  BtnBrowsePath.Visible := False;
  
  LabelConnectionString.Visible := False;
  EditConnectionString.Visible := False;
  
  // Show controls based on selected option
  if RadioCustomPath.Checked then
  begin
    LabelCustomPath.Visible := True;
    EditCustomPath.Visible := True;
    BtnBrowsePath.Visible := True;
    SelectedMongoOption := 2;
  end
  else if RadioConnectionString.Checked then
  begin
    LabelConnectionString.Visible := True;
    EditConnectionString.Visible := True;
    SelectedMongoOption := 3;
  end
  else
  begin
    SelectedMongoOption := 1; // Default
  end;
end;

// Event handler for Default radio button
procedure RadioDefaultClick(Sender: TObject);
begin
  UpdateMongoControlsVisibility;
end;

// Event handler for Custom Path radio button
procedure RadioCustomPathClick(Sender: TObject);
begin
  UpdateMongoControlsVisibility;
end;

// Event handler for Connection String radio button
procedure RadioConnectionStringClick(Sender: TObject);
begin
  UpdateMongoControlsVisibility;
end;



// Event handler for Browse button
procedure BtnBrowsePathClick(Sender: TObject);
var
  DirPath: String;
begin
  DirPath := EditCustomPath.Text;
  
  if BrowseForFolder('Select MongoDB Installation Directory:', DirPath, False) then
  begin
    EditCustomPath.Text := DirPath;
    CustomMongoPath := DirPath;
  end;
end;

function CopyLocalFile(const LocalFileName, TargetPath: string): Boolean;
var
  SetupDir: String;
  SourcePath: String;
begin
  Result := False;
  SetupDir := ExtractFilePath(ExpandConstant('{srcexe}'));
  SourcePath := SetupDir + 'Dependencies\' + LocalFileName;
  
  if FileExists(SourcePath) then
  begin
    Result := FileCopy(SourcePath, TargetPath, False);
    
  end
  else
  begin
    Log('Source file not found: ' + SourcePath);
  end;
end;

function IsPathInEnvironment(const Path: String): Boolean;
var
  CurrentPath: String;
begin
 //MsgBox('Warning: Could not add MongoDB Tools to system PATH. You may need to add it manually: ' + CurrentPath, mbInformation, MB_OK);
  Result := False;
  if RegQueryStringValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment', 'Path', CurrentPath) then
  begin
    // Convert to uppercase for case-insensitive comparison
    CurrentPath := UpperCase(CurrentPath);
    Result := Pos(UpperCase(Path), CurrentPath) > 0;
  end;
end;

function AddToSystemPath(const NewPath: String): Boolean;
var
  CurrentPath: String;
  UpdatedPath: String;
begin
  Result := False;
  
  // Check if path already exists
  if IsPathInEnvironment(NewPath) then
  begin
    Result := True;
    Exit;
  end;
  
  // Get current PATH
  if RegQueryStringValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment', 'Path', CurrentPath) then
  begin
    // Add new path to the end with semicolon separator
    if (Length(CurrentPath) > 0) and (CurrentPath[Length(CurrentPath)] <> ';') then
      UpdatedPath := CurrentPath + ';' + NewPath
    else
      UpdatedPath := CurrentPath + NewPath;
    
    // Update registry
    if RegWriteStringValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment', 'Path', UpdatedPath) then
    begin
      Result := True;
      // Notify system of environment change (WM_SETTINGCHANGE)
      // This may require a restart or re-login for all applications to see the change
      SendBroadcastMessage($001A, 0, 0);
    end;
  end;
end;

function SafeExec(const Filename, Params, WorkingDir: String; const ShowCmd: Integer; const Wait: TExecWait; var ResultCode: Integer): Boolean;
begin
  Result := False;
  try
    Result := Exec(Filename, Params, WorkingDir, ShowCmd, Wait, ResultCode);
  except
    
    begin
      
      ResultCode := -1;
    end;
  end;
end;

// Check if environment variable exists
function EnvironmentVariableExists(const Name: String): Boolean;
var
  Value: String;
begin
  Result := RegQueryStringValue(HKEY_LOCAL_MACHINE, 
    'SYSTEM\CurrentControlSet\Control\Session Manager\Environment', 
    Name, Value);
end;

// Set environment variable
function SetEnvironmentVariable(const Name, Value: String): Boolean;
begin
  Result := RegWriteStringValue(HKEY_LOCAL_MACHINE, 
    'SYSTEM\CurrentControlSet\Control\Session Manager\Environment', 
    Name, Value);
  
  if Result then
  begin
    // Notify system of environment change
    //SendBroadcastMessage($001A, 0, 0);
  end;
end;

function GetMajorMinorVersion(FullVersion: String): String;
var
  i, DotCount: Integer;
begin
  DotCount := 0;

  for i := 1 to Length(FullVersion) do
  begin
    if FullVersion[i] = '.' then
    begin
      Inc(DotCount);
      if DotCount = 2 then
      begin
        Result := Copy(FullVersion, 1, i - 1);
        Exit;
      end;
    end;
  end;

  Result := FullVersion;
end;


function GetUninstallCommand(ProductName: string): string;
var
  RootKeys: array[0..1] of Integer;
  UninstallKey: string;
  SubkeyName: string;
  UninstallString: string;
  I, J: Integer;
  SubkeyNames: TArrayOfString;
begin
  Result := '';
  RootKeys[0] := HKEY_LOCAL_MACHINE;
  RootKeys[1] := HKEY_CURRENT_USER;

  for I := 0 to 1 do
  begin
    if I = 1 then
      UninstallKey := 'SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall'
    else
      UninstallKey := 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall';

    if RegGetSubkeyNames(RootKeys[I], UninstallKey, SubkeyNames) then
    begin
      for J := 0 to GetArrayLength(SubkeyNames) - 1 do
      begin
        SubkeyName := SubkeyNames[J];

        if RegQueryStringValue(RootKeys[I], UninstallKey + '\' + SubkeyName, 'DisplayName', UninstallString) then
        begin
          if Pos(ProductName, UninstallString) > 0 then
          begin
            if RegQueryStringValue(RootKeys[I], UninstallKey + '\' + SubkeyName, 'UninstallString', Result) then
              Exit;
          end;
        end;
      end;
    end;
  end;
end;


// Create MongoDB configuration page
function CreateMongoConfigPage: TWizardPage;
var
  TopMargin: Integer;
begin
  Result := CreateCustomPage(wpSelectComponents, 
                             'MongoDB Configuration', 
                             'Choose how to configure MongoDB');
  
  TopMargin := 20;
  
  // Radio button for default installation
  RadioDefault := TRadioButton.Create(Result);
  RadioDefault.Parent := Result.Surface;
  RadioDefault.Left := 20;
  RadioDefault.Top := TopMargin;
  RadioDefault.Width := Result.SurfaceWidth - 40;
  RadioDefault.Height := ScaleY(17);
  RadioDefault.Caption := 'Use default MongoDB installation (C:\....)';
  RadioDefault.Checked := True;
  RadioDefault.OnClick := @RadioDefaultClick;
  
  // Radio button for custom path
  RadioCustomPath := TRadioButton.Create(Result);
  RadioCustomPath.Parent := Result.Surface;
  RadioCustomPath.Left := 20;
  RadioCustomPath.Top := TopMargin + ScaleY(40);
  RadioCustomPath.Width := Result.SurfaceWidth - 40;
  RadioCustomPath.Height := ScaleY(17);
  RadioCustomPath.Caption := 'Specify custom MongoDB installation path';
  RadioCustomPath.OnClick := @RadioCustomPathClick;
  
  // Label and edit box for custom path
  LabelCustomPath := TLabel.Create(Result);
  LabelCustomPath.Parent := Result.Surface;
  LabelCustomPath.Left := 40;
  LabelCustomPath.Top := TopMargin + ScaleY(65);
  LabelCustomPath.Caption := 'MongoDB Installation Path:';
  LabelCustomPath.Visible := False;
  
  EditCustomPath := TEdit.Create(Result);
  EditCustomPath.Parent := Result.Surface;
  EditCustomPath.Left := 40;
  EditCustomPath.Top := TopMargin + ScaleY(85);
  EditCustomPath.Width := Result.SurfaceWidth - 140;
  EditCustomPath.Visible := False;
  
  BtnBrowsePath := TButton.Create(Result);
  BtnBrowsePath.Parent := Result.Surface;
  BtnBrowsePath.Left := Result.SurfaceWidth - 80;
  BtnBrowsePath.Top := TopMargin + ScaleY(83);
  BtnBrowsePath.Width := ScaleX(75);
  BtnBrowsePath.Height := ScaleY(23);
  BtnBrowsePath.Caption := 'Browse...';
  BtnBrowsePath.OnClick := @BtnBrowsePathClick;
  BtnBrowsePath.Visible := False;
  
  // Radio button for connection string
  RadioConnectionString := TRadioButton.Create(Result);
  RadioConnectionString.Parent := Result.Surface;
  RadioConnectionString.Left := 20;
  RadioConnectionString.Top := TopMargin + ScaleY(120);
  RadioConnectionString.Width := Result.SurfaceWidth - 40;
  RadioConnectionString.Height := ScaleY(17);
  RadioConnectionString.Caption := 'Use existing MongoDB connection string';
  RadioConnectionString.OnClick := @RadioConnectionStringClick;
  
  // Label and edit box for connection string
  LabelConnectionString := TLabel.Create(Result);
  LabelConnectionString.Parent := Result.Surface;
  LabelConnectionString.Left := 40;
  LabelConnectionString.Top := TopMargin + ScaleY(145);
  LabelConnectionString.Caption := 'Connection String:';
  LabelConnectionString.Visible := False;
  
  EditConnectionString := TEdit.Create(Result);
  EditConnectionString.Parent := Result.Surface;
  EditConnectionString.Left := 40;
  EditConnectionString.Top := TopMargin + ScaleY(165);
  EditConnectionString.Width := Result.SurfaceWidth - 80;
  EditConnectionString.Visible := False;
end;

procedure InitializeWizard;
var
  InfoLabel: TLabel;
begin
  // Create custom components selection page
  ComponentsPage := CreateCustomPage(wpSelectDir, 'Select Components', 'Choose which components to install');
  
  // Add info label
  InfoLabel := TLabel.Create(ComponentsPage);
  InfoLabel.Parent := ComponentsPage.Surface;
  InfoLabel.Caption := 'The installer will check for existing installations and skip components that are already installed.';
  InfoLabel.Left := ScaleX(10);
  InfoLabel.Top := ScaleY(10);
  InfoLabel.Width := ScaleX(400);
  InfoLabel.Height := ScaleY(30);
  InfoLabel.WordWrap := True;
  InfoLabel.Font.Style := [fsBold];
  
  // IIS Setup checkbox
  IISSetupCheckBox := TCheckBox.Create(ComponentsPage);
  IISSetupCheckBox.Parent := ComponentsPage.Surface;
  IISSetupCheckBox.Caption := 'IIS Web Server and Application Setup (Web hosting configuration)';
  IISSetupCheckBox.Left := ScaleX(10);
  IISSetupCheckBox.Top := ScaleY(60);
  IISSetupCheckBox.Width := ScaleX(400);
  IISSetupCheckBox.Height := ScaleY(17);
  IISSetupCheckBox.Checked := True;
  
  // .NET Hosting Bundle checkbox
  NetHostingCheckBox := TCheckBox.Create(ComponentsPage);
  NetHostingCheckBox.Parent := ComponentsPage.Surface;
  NetHostingCheckBox.Caption := 'ASP.NET Core Hosting Bundle 9.0 (Required for web application)';
  NetHostingCheckBox.Left := ScaleX(10);
  NetHostingCheckBox.Top := ScaleY(80);
  NetHostingCheckBox.Width := ScaleX(400);
  NetHostingCheckBox.Height := ScaleY(17);
  NetHostingCheckBox.Checked := True;
  
    // .NET Hosting Bundle checkbox
  InstallDotNetSDKCheckBox := TCheckBox.Create(ComponentsPage);
  InstallDotNetSDKCheckBox.Parent := ComponentsPage.Surface;
  InstallDotNetSDKCheckBox.Caption := 'ASP.NET SDK 8.0.412 (Required for web application)';
  InstallDotNetSDKCheckBox.Left := ScaleX(10);
  InstallDotNetSDKCheckBox.Top := ScaleY(100);
  InstallDotNetSDKCheckBox.Width := ScaleX(400);
  InstallDotNetSDKCheckBox.Height := ScaleY(17);
  InstallDotNetSDKCheckBox.Checked := True;
  
  // MongoDB checkbox
  MongoDBCheckBox := TCheckBox.Create(ComponentsPage);
  MongoDBCheckBox.Parent := ComponentsPage.Surface;
  MongoDBCheckBox.Caption := 'MongoDB Server 7.0.12 (Database server)';
  MongoDBCheckBox.Left := ScaleX(10);
  MongoDBCheckBox.Top := ScaleY(120);
  MongoDBCheckBox.Width := ScaleX(400);
  MongoDBCheckBox.Height := ScaleY(17);
  MongoDBCheckBox.Checked := True;
  
  // MongoDB Tools checkbox
  MongoToolsCheckBox := TCheckBox.Create(ComponentsPage);
  MongoToolsCheckBox.Parent := ComponentsPage.Surface;
  MongoToolsCheckBox.Caption := 'MongoDB Shell (mongosh) 2.3.0 (Database management tool)';
  MongoToolsCheckBox.Left := ScaleX(10);
  MongoToolsCheckBox.Top := ScaleY(140);
  MongoToolsCheckBox.Width := ScaleX(400);
  MongoToolsCheckBox.Height := ScaleY(17);
  MongoToolsCheckBox.Checked := True;
  
  // Create desktop shortcut checkbox on finish page
  CreateShortcutCheckBox := TCheckBox.Create(WizardForm);
  CreateShortcutCheckBox.Parent := WizardForm.FinishedPage;
  CreateShortcutCheckBox.Caption := 'Create a desktop shortcut to Vision Insight';
  CreateShortcutCheckBox.Checked := True;
  CreateShortcutCheckBox.Left := ScaleX(180);
  CreateShortcutCheckBox.Top := ScaleY(180);
  CreateShortcutCheckBox.Width := ScaleX(400);
  CreateShortcutCheckBox.Height := ScaleY(17);
  
    
  // Create checkbox for restart on finish page
  InstallSuccessful := False; // Default to false
  RestartCheckBox := TCheckBox.Create(WizardForm);
  RestartCheckBox.Parent := WizardForm.FinishedPage;
  RestartCheckBox.Caption := 'A system restart is recommended to apply all changes.';
  RestartCheckBox.Checked := True;
  RestartCheckbox.Visible := True;
  RestartCheckBox.Left := ScaleX(180);
  RestartCheckBox.Top := ScaleY(200);
  RestartCheckBox.Width := ScaleX(400);
  RestartCheckBox.Height := ScaleY(17);
  
   // Create MongoDB Configuration Page
    // Apply MongoDB configuration BEFORE other installations
    if  MongoDBCheckBox.Checked then
    begin
        //ApplyMongoConfiguration;
      //CreateMongoConfigPage;
      MongoConfigPage := CreateMongoConfigPage;
    end;
  
end;

// Check if any installed DisplayName starts with the given ProgramName
function IsProgramInstalled(const ProgramNamePrefix: string): Boolean;
var
  SubKeys: TArrayOfString;
  i: Integer;
  DisplayName: string;
  RegPaths: TArrayOfString;
  j: Integer;
begin
  Result := False;

    // First, check the direct MongoDB registry path
  if RegKeyExists(HKEY_LOCAL_MACHINE, 'SOFTWARE\MongoDB\' + ProgramNamePrefix) then
  begin
    Result := True;
    Exit;
  end;
  
  // Also check in WOW6432Node for 32-bit apps on 64-bit Windows
  if RegKeyExists(HKEY_LOCAL_MACHINE, 'SOFTWARE\WOW6432Node\MongoDB\' + ProgramNamePrefix) then
  begin
    Result := True;
    Exit;
  end;
  
  SetArrayLength(RegPaths, 2);
  RegPaths[0] := 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall';
  RegPaths[1] := 'SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall';
  
  for j := 0 to GetArrayLength(RegPaths) - 1 do
  begin
    if RegGetSubkeyNames(HKEY_LOCAL_MACHINE, RegPaths[j], SubKeys) then
    begin
      for i := 0 to GetArrayLength(SubKeys) - 1 do
      begin
        if RegQueryStringValue(HKEY_LOCAL_MACHINE, RegPaths[j] + '\' + SubKeys[i], 'DisplayName', DisplayName) then
        begin
          // Check if DisplayName starts with the prefix
          if Pos(Uppercase(ProgramNamePrefix), Uppercase(DisplayName)) = 1 then
          begin
            Result := True;
            Exit;
          end;
        end;
      end;
    end;
  end;
end;

function FindMongoShell: String;
var
  PossiblePaths: array[0..5] of String;
  I: Integer;
begin
  Result := '';
  
  // Define all possible mongosh locations
  PossiblePaths[0] := 'C:\Program Files\mongosh\bin\mongosh.exe';
  PossiblePaths[1] := 'C:\Program Files\MongoDB\mongosh\mongosh.exe';
  PossiblePaths[2] := 'C:\Program Files\MongoDB\Server\7.0\bin\mongosh.exe';
  PossiblePaths[3] := ExpandConstant('{pf}\mongosh\bin\mongosh.exe');
  PossiblePaths[4] := ExpandConstant('{pf}\MongoDB\mongosh\mongosh.exe');
  PossiblePaths[5] := 'C:\Users\' + ExpandConstant('{username}') + '\AppData\Local\Programs\mongosh\mongosh.exe';
  
  // Search for mongosh
  for I := 0 to 5 do
  begin
    if FileExists(PossiblePaths[I]) then
    begin
      Result := PossiblePaths[I];
      Log('Found mongosh at: ' + Result);
      Exit;
    end;
  end;
  
  Log('mongosh not found in any standard location');
end;

function IsMongoDBServiceRunning(MongoShellPath: String): Boolean;
var
  ResultCode: Integer;
  CheckServiceCmd: String;
begin
  Result := False;
  
  Log('Checking if MongoDB service is up and running...');
  
  CheckServiceCmd := 'try { ' +
                     '  $output = & "' + MongoShellPath + '" --quiet --eval "db.runCommand({ hello: 1 }).ok" 2>&1; ' +
                     '  Write-Host "Service check output: $output"; ' +
                     '  if ($output -match "1") { ' +
                     '    Write-Host "MongoDB is running"; ' +
                     '    exit 0 ' +
                     '  } else { ' +
                     '    Write-Host "MongoDB is not responding correctly"; ' +
                     '    exit 1 ' +
                     '  } ' +
                     '} catch { ' +
                     '  Write-Host "Service check error: $_"; ' +
                     '  exit 1 ' +
                     '}';
  
  Log('CheckServiceCmd = ' + CheckServiceCmd);
  
  if SafeExec('powershell.exe', '-NoProfile -ExecutionPolicy Bypass -Command "' + CheckServiceCmd + '"',
              '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Log('Service check completed. Exit code: ' + IntToStr(ResultCode));
    if ResultCode = 0 then
    begin
      Log('MongoDB service is UP and RUNNING');
      Result := True;
    end
    else
    begin
      Log('MongoDB service check failed with exit code: ' + IntToStr(ResultCode));
      Result := False;
    end;
  end
  else
  begin
    Log('Failed to execute service check command');
    Result := False;
  end;
end;

function IsMongoDBInstalled: Boolean;
begin
  Result := IsProgramInstalled('MongoDB {#MongoDBVersion}');
end;

function IsNetHostingInstalled: Boolean;
begin
  Result := IsProgramInstalled('Microsoft .NET {#DotNetHostingVersion}');
end;

function IsDotNetSDKInstalled: Boolean;
begin
  Result := IsProgramInstalled('Microsoft ASP.NET Core {#DotNetSDKVersion} - Shared Framework (x64)');
end;

//function IsMongoToolsInstalled: Boolean;
//begin
//  Result := IsProgramInstalled('MongoDB Tools 100');
//end;

function IsMongoToolsInstalled: Boolean;
var
  InstallPath: string;
  ExePath: string;
begin
  Result := False;
  
  // Check the exact registry path found by PS1
  if RegKeyExists(HKEY_LOCAL_MACHINE, 'SOFTWARE\MongoDB\MongoDB Tools 100') then
  begin
    // Try to get the installation path from registry
    if RegQueryStringValue(HKEY_LOCAL_MACHINE, 'SOFTWARE\MongoDB\MongoDB Tools 100', 'InstallPath', InstallPath) or
       RegQueryStringValue(HKEY_LOCAL_MACHINE, 'SOFTWARE\MongoDB\MongoDB Tools 100', 'InstallLocation', InstallPath) then
    begin
      // Verify the actual executable exists
      ExePath := AddBackslash(InstallPath) + 'bin\mongodump.exe';
      if FileExists(ExePath) then
      begin
        Result := True;
        Exit;
      end;
    end;
    
    // If registry exists but no valid path, check common locations
    if FileExists(ExpandConstant('{pf}\MongoDB\Tools\100\bin\mongodump.exe')) then
    begin
      Result := True;
      Exit;
    end;
  end;
  
  // Also check WOW6432Node (for 32-bit apps on 64-bit Windows)
  if RegKeyExists(HKEY_LOCAL_MACHINE, 'SOFTWARE\WOW6432Node\MongoDB\MongoDB Tools 100') then
  begin
    if RegQueryStringValue(HKEY_LOCAL_MACHINE, 'SOFTWARE\WOW6432Node\MongoDB\MongoDB Tools 100', 'InstallPath', InstallPath) or
       RegQueryStringValue(HKEY_LOCAL_MACHINE, 'SOFTWARE\WOW6432Node\MongoDB\MongoDB Tools 100', 'InstallLocation', InstallPath) then
    begin
      ExePath := AddBackslash(InstallPath) + 'bin\mongodump.exe';
      if FileExists(ExePath) then
      begin
        Result := True;
        Exit;
      end;
    end;
    
    if FileExists(ExpandConstant('{pf32}\MongoDB\Tools\100\bin\mongodump.exe')) then
    begin
      Result := True;
      Exit;
    end;
  end;
end;

function IsMongoShellInstalled: Boolean;
begin
  Result := IsProgramInstalled('MongoDB Shell');
end;


// Functions to install components
function SetupIIS: Boolean;
var
  ResultCode: Integer;
  IISScript: String;
  PhysicalPath: String;
  AppPoolName: String;
  SiteName: String;
  Port: String;
  DefaultSiteName: String;
  LogFile: String;
  ScriptFile: String;
begin
  Result := True;
  if not IISSetupCheckBox.Checked then Exit;
  
  // Configuration variables
  AppPoolName := '{#AppPoolName}';
  SiteName := '{#SiteName}';
  Port := '{#SitePort}';
  DefaultSiteName := '{#DefaultSiteName}';
  PhysicalPath := ExpandConstant('{app}\Publish');
  LogFile := ExpandConstant('{tmp}\IIS_Setup_Log.txt');
  ScriptFile := ExpandConstant('{tmp}\IIS_Setup_Script.ps1');
  
  ProgressPage.SetText('Checking IIS installation...', 'Detecting existing IIS setup...');
  ProgressPage.SetProgress(75, 100);
  
  // Create folder if it doesn't exist
  if not DirExists(PhysicalPath) then
    ForceDirectories(PhysicalPath);
  
  ProgressPage.SetText('Creating PowerShell script...', 'Preparing IIS setup script...');
  ProgressPage.SetProgress(78, 100);
  
  // Create the PowerShell script file with your exact functions
  IISScript := 
    '$ErrorActionPreference = "Stop"' + #13#10 +
    '$LogFile = "' + LogFile + '"' + #13#10 +
    '$AppPoolName = "' + AppPoolName + '"' + #13#10 +
    '$SiteName = "' + SiteName + '"' + #13#10 +
    '$Port = ' + Port + #13#10 +
    '$PhysicalPath = "' + PhysicalPath + '"' + #13#10 +
    '$DefaultSiteName = "' + DefaultSiteName + '"' + #13#10 + #13#10 +
    
    'function Write-Log { param([string]$Message); $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"; "$timestamp - $Message" | Out-File -FilePath $LogFile -Append -Encoding UTF8; Write-Host $Message; }' + #13#10 + #13#10 +
    
    'try {' + #13#10 +
    '    Write-Log "=== IIS Setup Process Started ==="' + #13#10 +
    '    Write-Log "App Pool: $AppPoolName"' + #13#10 +
    '    Write-Log "Site Name: $SiteName"' + #13#10 +
    '    Write-Log "Port: $Port"' + #13#10 +
    '    Write-Log "Physical Path: $PhysicalPath"' + #13#10 + #13#10 +
    
    // Ensure-IISDependencies function
    'function Ensure-IISDependencies {' + #13#10 +
    '    Write-Log "Starting IIS Dependencies Check..."' + #13#10 +
    '    $features = @("IIS-WebServerRole", "IIS-WebServer", "IIS-WebMgmtTools",  "IIS-ManagementScriptingTools", "IIS-ManagementService")' + #13#10 +
    '    foreach ($feature in $features) {' + #13#10 +
    '        Write-Log "Loop $feature is started."' + #13#10 +
    '        $featureStatus = (dism /online /Get-FeatureInfo /FeatureName:$feature 2>&1 | Select-String "State :")' + #13#10 +
    '        if ($featureStatus -and $featureStatus -match "Enabled") {' + #13#10 +
    '            Write-Log "$feature is already enabled."' + #13#10 +
    '        } else {' + #13#10 +
    '            Write-Log "Enabling: $feature..."' + #13#10 +
    '            Start-Process "dism.exe" -ArgumentList "/online", "/Enable-Feature", "/FeatureName:$feature", "/All", "/NoRestart" -Wait -WindowStyle Hidden' + #13#10 +
    '        }' + #13#10 +
    '    }' + #13#10 +
    '}' + #13#10 + #13#10 +
    
    // Import WebAdministration after ensuring IIS is installed
        '    Ensure-IISDependencies' + #13#10 + #13#10 +

    // SAFE IMPORT OF MODULE
    '$attempts = 0' + #13#10 +
    'while (-not (Get-Module -ListAvailable -Name WebAdministration) -and $attempts -lt 6) {' + #13#10 +
    '    Write-Log "Waiting for WebAdministration module to become available..."' + #13#10 +
    '    Start-Sleep -Seconds 5' + #13#10 +
    '    $attempts++' + #13#10 +
    '}' + #13#10 +
    'try {' + #13#10 +
    '    Import-Module WebAdministration -Force -ErrorAction Stop' + #13#10 +
    '    Write-Log "WebAdministration module imported successfully."' + #13#10 +
    '} catch {' + #13#10 +
    '    Write-Log "ERROR: Failed to import WebAdministration module: $($_.Exception.Message)"' + #13#10 +
    '    throw "WebAdministration module could not be loaded."' + #13#10 +
    '}' + #13#10 + #13#10 +

    
    // Create-AppPool function
    'function Create-AppPool {' + #13#10 +
    '    Write-Log "==================== Starting IIS Hosting Setup ===================="' + #13#10 +
    '    try {' + #13#10 +
    '        Import-Module WebAdministration -Force' + #13#10 +
    '        Write-Log "WebAdministration module imported successfully."' + #13#10 +
    '    } catch {' + #13#10 +
    '        Write-Log "Failed to import WebAdministration module: $($_.Exception.Message)"' + #13#10 +
    '        Write-Log "Attempting to enable IIS features..."' + #13#10 +
    '        Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole, IIS-WebServer, IIS-CommonHttpFeatures, IIS-HttpErrors, IIS-HttpLogging, IIS-RequestMonitor, IIS-HttpTracing, IIS-Security, IIS-RequestFiltering, IIS-Performance, IIS-WebServerManagementTools, IIS-ManagementConsole, IIS-IIS6ManagementCompatibility, IIS-Metabase -All' + #13#10 +
    '        Import-Module WebAdministration -Force' + #13#10 +
    '    }' + #13#10 +
    '    Write-Log "Checking if Application Pool ''$AppPoolName'' exists..."' + #13#10 +
    '    try {' + #13#10 +
    '        $existingPool = Get-IISAppPool -Name $AppPoolName -ErrorAction SilentlyContinue' + #13#10 +
    '        if ($existingPool) {' + #13#10 +
    '            Write-Log "Application Pool ''$AppPoolName'' already exists."' + #13#10 +
    '        } else {' + #13#10 +
    '            New-WebAppPool -Name $AppPoolName' + #13#10 +
    '            Write-Log "Application Pool ''$AppPoolName'' created successfully."' + #13#10 +
    '        }' + #13#10 +
    '    } catch {' + #13#10 +
    '        Write-Log "Using appcmd.exe as fallback method..."' + #13#10 +
    '        $appcmdPath = "$env:SystemRoot\System32\inetsrv\appcmd.exe"' + #13#10 +
    '        if (Test-Path $appcmdPath) {' + #13#10 +
    '            $result = & $appcmdPath list apppool $AppPoolName 2>$null' + #13#10 +
    '            if ($result) {' + #13#10 +
    '                Write-Log "Application Pool ''$AppPoolName'' already exists."' + #13#10 +
    '            } else {' + #13#10 +
    '                & $appcmdPath add apppool /name:$AppPoolName' + #13#10 +
    '                Write-Log "Application Pool ''$AppPoolName'' created successfully using appcmd.exe."' + #13#10 +
    '            }' + #13#10 +
    '        } else {' + #13#10 +
    '            Write-Log "ERROR: Neither PowerShell cmdlets nor appcmd.exe are available for IIS management."' + #13#10 +
    '            throw "IIS management tools are not available."' + #13#10 +
    '        }' + #13#10 +
    '    }' + #13#10 +
    '}' + #13#10 + #13#10 +
    
    // Stop-DefaultWebsite function using appcmd.exe
    'function Stop-DefaultWebsite {' + #13#10 +
    '    param ([string]$DefaultSiteName)' + #13#10 +
    '    Write-Log "Stopping Default Web Site..."' + #13#10 +
    '    try {' + #13#10 +
    '        Stop-Website -Name $DefaultSiteName -ErrorAction Stop' + #13#10 +
    '        Write-Log "Default Web Site stopped using PowerShell cmdlet."' + #13#10 +
    '    } catch {' + #13#10 +
    '        Write-Log "PowerShell cmdlet failed: $($_.Exception.Message). Using appcmd.exe..."' + #13#10 +
    '        $appcmdPath = "$env:SystemRoot\System32\inetsrv\appcmd.exe"' + #13#10 +
    '        if (Test-Path $appcmdPath) {' + #13#10 +
    '            $result = & $appcmdPath stop site "$DefaultSiteName" 2>&1' + #13#10 +
    '            if ($LASTEXITCODE -eq 0) {' + #13#10 +
    '                Write-Log "Default Web Site stopped using appcmd.exe."' + #13#10 +
    '            } else {' + #13#10 +
    '                Write-Log "appcmd.exe result: $result (Exit code: $LASTEXITCODE)"' + #13#10 +
    '                Write-Log "Note: Default site may already be stopped or not exist."' + #13#10 +
    '            }' + #13#10 +
    '        } else {' + #13#10 +
    '            Write-Log "WARNING: appcmd.exe not found. Cannot stop default website."' + #13#10 +
    '        }' + #13#10 +
    '    }' + #13#10 +
    '}' + #13#10 + #13#10 +
    
    // Create-Website function (simplified version)
    'function Create-Website {' + #13#10 +
    '    param ([string]$SiteName, [string]$PhysicalPath, [int]$Port, [string]$AppPoolName)' + #13#10 +
    '    Write-Log "==================== Creating/Updating IIS Website ===================="' + #13#10 +
    '    Write-Log "Site Name: $SiteName"' + #13#10 +
    '    Write-Log "Physical Path: $PhysicalPath"' + #13#10 +
    '    Write-Log "Port: $Port"' + #13#10 +
    '    Write-Log "Application Pool: $AppPoolName"' + #13#10 +
    '    $appcmdPath = "$env:SystemRoot\System32\inetsrv\appcmd.exe"' + #13#10 +
    '    if (!(Test-Path $appcmdPath)) {' + #13#10 +
    '        Write-Log "ERROR: IIS is not installed or appcmd.exe is not available."' + #13#10 +
    '        return $false' + #13#10 +
    '    }' + #13#10 +
    '    if (!(Test-Path $PhysicalPath)) {' + #13#10 +
    '        New-Item -ItemType Directory -Path $PhysicalPath -Force | Out-Null' + #13#10 +
    '        Write-Log "Created physical path: $PhysicalPath"' + #13#10 +
    '    }' + #13#10 +
    '    $siteList = & $appcmdPath list site $SiteName 2>$null' + #13#10 +
    '    $siteExists = ($siteList -and $siteList.Length -gt 0 -and $siteList -notlike "*ERROR*")' + #13#10 +
    '    if ($siteExists) {' + #13#10 +
    '        Write-Log "Website ''$SiteName'' exists. Updating configuration..."' + #13#10 +
    '        & $appcmdPath stop site $SiteName 2>$null' + #13#10 +
    '        & $appcmdPath set vdir "${SiteName}/" /physicalPath:$PhysicalPath 2>&1' + #13#10 +
    '        & $appcmdPath set app "${SiteName}/" /applicationPool:$AppPoolName 2>&1' + #13#10 +
    '        $bindingString = "http/*:${Port}:"' + #13#10 +
    '        & $appcmdPath set site $SiteName /bindings:$bindingString 2>&1' + #13#10 +
    '        & $appcmdPath start site $SiteName 2>$null' + #13#10 +
    '    } else {' + #13#10 +
    '        Write-Log "Website ''$SiteName'' does not exist. Creating new website..."' + #13#10 +
    '        $bindingString = "http/*:${Port}:"' + #13#10 +
    '        & $appcmdPath add site /name:$SiteName /physicalPath:$PhysicalPath /bindings:$bindingString 2>&1' + #13#10 +
    '        & $appcmdPath set app "${SiteName}/" /applicationPool:$AppPoolName 2>&1' + #13#10 +
    '        & $appcmdPath start site $SiteName 2>&1' + #13#10 +
    '        Write-Log "Website URL: http://localhost:$Port"' + #13#10 +
    '    }' + #13#10 +
    '    return $true' + #13#10 +
    '}' + #13#10 + #13#10 +
    
    // Configure-Permissions function
    'function Configure-Permissions {' + #13#10 +
    '    param ([string]$PhysicalPath)' + #13#10 +
    '    Write-Log "Configuring folder permissions for IIS_IUSRS..."' + #13#10 +
    '    try {' + #13#10 +
    '        $Acl = Get-Acl $PhysicalPath' + #13#10 +
    '        $AccessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")' + #13#10 +
    '        $Acl.SetAccessRule($AccessRule)' + #13#10 +
    '        Set-Acl -Path $PhysicalPath -AclObject $Acl' + #13#10 +
    '        Write-Log "Permissions successfully configured for IIS_IUSRS."' + #13#10 +
    '    } catch {' + #13#10 +
    '        Write-Log "Failed to configure permissions. Error: $_"' + #13#10 +
    '    }' + #13#10 +
    '}' + #13#10 + #13#10 +
    
    // Start-WebsiteSafely function using appcmd.exe fallback
    'function Start-WebsiteSafely {' + #13#10 +
    '    param ([string]$SiteName)' + #13#10 +
    '    Write-Log "Starting IIS Website ''$SiteName''..."' + #13#10 +
    '    try {' + #13#10 +
    '        Start-Website -Name $SiteName -ErrorAction Stop' + #13#10 +
    '        Write-Log "Website ''$SiteName'' started successfully using PowerShell cmdlet."' + #13#10 +
    '    } catch {' + #13#10 +
    '        Write-Log "PowerShell cmdlet failed: $($_.Exception.Message). Using appcmd.exe..."' + #13#10 +
    '        $appcmdPath = "$env:SystemRoot\System32\inetsrv\appcmd.exe"' + #13#10 +
    '        if (Test-Path $appcmdPath) {' + #13#10 +
    '            $result = & $appcmdPath start site "$SiteName" 2>&1' + #13#10 +
    '            if ($LASTEXITCODE -eq 0) {' + #13#10 +
    '                Write-Log "Website ''$SiteName'' started successfully using appcmd.exe."' + #13#10 +
    '            } else {' + #13#10 +
    '                Write-Log "Failed to start website using appcmd.exe: $result (Exit code: $LASTEXITCODE)"' + #13#10 +
    '                throw "Failed to start website $SiteName"' + #13#10 +
    '            }' + #13#10 +
    '        } else {' + #13#10 +
    '            Write-Log "ERROR: appcmd.exe not found. Cannot start website."' + #13#10 +
    '            throw "Cannot start website - no available method"' + #13#10 +
    '        }' + #13#10 +
    '    }' + #13#10 +
    '    Write-Log "==================== IIS hosting setup completed successfully! ===================="' + #13#10 +
    '}' + #13#10 + #13#10 +
    
    // Execute functions in sequence (like your PS1 file)
    '    Write-Log "Calling Ensure-IISDependencies..."' + #13#10 +
    '    Ensure-IISDependencies' + #13#10 + #13#10 +
    
    '    Write-Log "Calling Create-AppPool..."' + #13#10 +
    '    Create-AppPool -AppPoolName $AppPoolName' + #13#10 + #13#10 +
    
    '    Write-Log "Calling Stop-DefaultWebsite..."' + #13#10 +
    '    Stop-DefaultWebsite -DefaultSiteName $DefaultSiteName' + #13#10 + #13#10 +
    
    '    Write-Log "Calling Create-Website..."' + #13#10 +
    '    Create-Website -SiteName $SiteName -PhysicalPath $PhysicalPath -Port $Port -AppPoolName $AppPoolName' + #13#10 + #13#10 +
    
    '    Write-Log "Calling Configure-Permissions..."' + #13#10 +
    '    Configure-Permissions -PhysicalPath $PhysicalPath' + #13#10 + #13#10 +
    
    '    Write-Log "Calling Start-WebsiteSafely..."' + #13#10 +
    '    Start-WebsiteSafely -SiteName $SiteName' + #13#10 + #13#10 +
    
    '    Write-Log "IIS setup completed successfully!"' + #13#10 +
    '} catch {' + #13#10 +
    '    Write-Log "Error setting up IIS: $_"' + #13#10 +
    '    Write-Log "Exception details: $($_.Exception.Message)"' + #13#10 +
    '    Write-Log "Stack trace: $($_.ScriptStackTrace)"' + #13#10 +
    '    exit 1' + #13#10 +
    '}';
  
  // Save the script to a file
  if not SaveStringToFile(ScriptFile, IISScript, False) then
  begin
    MsgBox('Failed to create PowerShell script file.', mbError, MB_OK);
    Result := False;
    Exit;
  end;
  
  ProgressPage.SetText('Setting up IIS and Web Application...', 'Configuring web server and application...');
  ProgressPage.SetProgress(85, 100);
  
  // Execute the script file
  if not Exec('powershell.exe', '-ExecutionPolicy Bypass -File "' + ScriptFile + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    MsgBox('IIS setup execution failed.', mbError, MB_OK);
    Result := False;
    Exit;
  end;
  
  // Handle result codes with log file information
  if ResultCode <> 0 then
  begin
    if FileExists(LogFile) then
    begin
      MsgBox('IIS setup failed with exit code: ' + IntToStr(ResultCode) + #13#10 + #13#10 +
             'Check the detailed log file:' + #13#10 +
             LogFile + #13#10 + #13#10 +
             'Script file saved at:' + #13#10 +
             ScriptFile + #13#10 + #13#10 +
             'You can run the script manually as Administrator to see detailed error messages.', mbError, MB_OK);
    end
    else
    begin
      MsgBox('IIS setup failed with exit code: ' + IntToStr(ResultCode) + #13#10 + #13#10 +
             'Script file saved at:' + #13#10 +
             ScriptFile + #13#10 + #13#10 +
             'Run this script manually as Administrator:' + #13#10 +
             'powershell.exe -ExecutionPolicy Bypass -File "' + ScriptFile + '"', mbError, MB_OK);
    end;
    Result := False;
    Exit;
  end;
  
  ProgressPage.SetText('IIS setup completed successfully', 'Web application is ready');
  ProgressPage.SetProgress(95, 100);
end;

function InstallNetHostingBundle: Boolean;
var
  ResultCode: Integer;
  InstallerPath: String;
begin
  Result := True;
  if not NetHostingCheckBox.Checked then Exit;
  
  try
    ProgressPage.SetText('Checking ASP.NET Core Hosting Bundle...', 'Detecting existing installation...');
    ProgressPage.SetProgress(5, 100);
    
    // Check if already installed
    if IsNetHostingInstalled then
    begin
      ProgressPage.SetText('ASP.NET Core Hosting Bundle already installed.', 'Skipping installation...');
      ProgressPage.SetProgress(25, 100);
      Sleep(1000);
      Exit;
    end;
    
    ProgressPage.SetText('Installing ASP.NET Core Hosting Bundle...', 'Please wait while the hosting bundle is being installed...');
    ProgressPage.SetProgress(10, 100);
    
    InstallerPath := ExpandConstant('{tmp}\{#DotNetHostingFile}');
    
    // Copy local file to temp
    if not CopyLocalFile('{#DotNetHostingFile}', InstallerPath) then
    begin
      MsgBox('Failed to copy .NET Hosting Bundle installer from Dependencies folder.', mbError, MB_OK);
      Result := False;
      Exit;
    end;
    
    if not SafeExec(InstallerPath, '/install /quiet /norestart', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
    begin
      MsgBox('Failed to execute ASP.NET Core Hosting Bundle installer. Error code: ' + IntToStr(ResultCode), mbError, MB_OK);
      Result := False;
    end
    else if ResultCode <> 0 then
    begin
      MsgBox('ASP.NET Core Hosting Bundle installation failed with exit code: ' + IntToStr(ResultCode), mbError, MB_OK);
      Result := False;
    end;
    
    ProgressPage.SetProgress(25, 100);
  except
    
    begin
      Result := False;
    end;
  end;
end;


function InstallDotNetSDK: Boolean;
var
  ResultCode: Integer;
  InstallerPath: String;
begin
  Result := True;
  
  if not InstallDotNetSDKCheckBox.Checked then
  begin
    Log('.NET SDK installation skipped by user');
    Exit;
  end;
  
  if IsDotNetSDKInstalled then
  begin
    Log('.NET SDK is already installed');
   // MsgBox('.NET SDK {#DotNetSDKVersion} is already installed on this system.', mbInformation, MB_OK);
     ProgressPage.SetProgress(25, 100);
      Sleep(1000);
    Exit;
  end;
  
  
  InstallerPath := ExpandConstant('{tmp}\{#DotNetSDKFile}');

    // Copy from local EXE folder to temp
  if not CopyLocalFile('{#DotNetSDKFile}', InstallerPath) then
  begin
    MsgBox('Failed to copy .NET SDK installer from local directory.', mbError, MB_OK);
    Result := False;
    Exit;
  end;

  ProgressPage.SetText('Installing .NET SDK...', 'Please wait while .NET SDK {#DotNetSDKVersion} is being installed...');
  ProgressPage.SetProgress(30, 100);

  
  if not SafeExec(InstallerPath, '/install /quiet /norestart', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    MsgBox('Failed to install .NET SDK.', mbError, MB_OK);
    Result := False;
    Exit;
  end;
  
  if ResultCode <> 0 then
  begin
    MsgBox('Warning: .NET SDK installation returned exit code: ' + IntToStr(ResultCode) + 
           '. The installation may require a system restart.', mbInformation, MB_OK);
  end;
  
  Log('.NET SDK installation completed');
end;

function InstallMongoDB: Boolean;
var
  ResultCode: Integer;
  InstallerPath: String;
  InstallPath: String;
  MongoBinPath: String;
  MongoDBFolderVersion: String;
begin
  Result := True;
  if not MongoDBCheckBox.Checked then Exit;

  ProgressPage.SetText('Checking MongoDB Server...', 'Detecting existing installation...');
  ProgressPage.SetProgress(30, 100);

  if SelectedMongoOption = 3 then Exit;

  // Stronger skip logic: Check service, folder, or executable
  if IsMongoDBInstalled then
  begin
    ProgressPage.SetText('MongoDB Server already installed.', 'Skipping installation...');
    ProgressPage.SetProgress(50, 100);
    //MsgBox('MongoDB Server already installed.', mbConfirmation, MB_OK);
    Sleep(1000);
    Exit;
  end;

  ProgressPage.SetText('Installing MongoDB Server...', 'Please wait while MongoDB is being installed...');
  ProgressPage.SetProgress(35, 100);

  InstallerPath := ExpandConstant('{tmp}\{#MongoDBFile}');

  MongoDBFolderVersion := GetMajorMinorVersion('{#MongoDBVersion}');
  
   //MsgBox(CustomMongoPath, mbInformation, MB_OK);
  // Determine installation path based on user selection
  if SelectedMongoOption = 2 then
  begin
    // Custom Path selected
    InstallPath := CustomMongoPath;
    if Copy(InstallPath, Length(InstallPath), 1) <> '\' then
      InstallPath := InstallPath + '\MongoDB\Server\'+MongoDBFolderVersion+'\';
    //MongoBinPath := InstallPath + 'bin';
    //DataPath := InstallPath + 'data';
    Log('Installing MongoDB to custom path: ' + InstallPath);
  end
  else
  begin
    // Default path
    InstallPath := 'C:\Program Files\MongoDB\Server\'+MongoDBFolderVersion+'\';
   // DataPath := 'C:\data';
    Log('Installing MongoDB to default path: ' + InstallPath);
  end;
    MongoBinPath := InstallPath + 'bin';

  // Check if install folder already exists
  if DirExists(InstallPath) then
  begin
    ProgressPage.SetText('MongoDB directory already exists.', 'Skipping MongoDB installation to avoid conflict...');
    ProgressPage.SetProgress(40, 100);
    Exit;
  end;
  
 Log('DirExists(InstallPath) ' + InstallPath);
  // Copy local file to temp
  if not CopyLocalFile('{#MongoDBFile}', InstallerPath) then
  begin
    MsgBox('Failed to copy MongoDB installer from Dependencies folder.', mbError, MB_OK);
    Result := False;
    Exit;
  end;

  // Run installer with verbose log
  if not Exec('msiexec.exe', Format('/i "%s" INSTALLLOCATION="%s" ADDLOCAL=all /quiet /norestart /l*v "%s"',        [InstallerPath, InstallPath, ExpandConstant('{tmp}\mongodb_install.log')]), '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    MsgBox('MongoDB installation failed to execute.', mbError, MB_OK);
    Result := False;
    Exit;
  end;
  
  // Add MongoDB bin path to system PATH
  if not IsPathInEnvironment(MongoBinPath) then
  begin
    ProgressPage.SetText('Adding MongoDB Server to PATH...', 'Updating environment variables...');
    if AddToSystemPath(MongoBinPath) then
      ProgressPage.SetText('MongoDB Server PATH updated.', 'Environment variables updated successfully.')
  end;

  Log('IsPathInEnvironment MongoBinPath' + MongoBinPath);
  
  // Handle known failure
  if ResultCode = 1603 then
  begin
    MsgBox('MongoDB installation failed with exit code: 1603.' + #13#10 +
           'A possible cause is that MongoDB is already installed or partially installed.' + #13#10 +
           'Check if a MongoDB service exists or remove the folder manually:' + #13#10 +
           InstallPath, mbError, MB_OK);
    Result := False;
    Exit;
  end;

  ProgressPage.SetProgress(50, 100);
end;

function InstallMongoShell: Boolean;
var
  ResultCode: Integer;
  InstallerPath: String;
  LogFile: String;
  MongoShellPath: String;
begin
  Result := True;
  if not MongoToolsCheckBox.Checked then Exit;

  if SelectedMongoOption = 3 then Exit;

  LogFile := ExpandConstant('{tmp}\mongosh_install.log');

  ProgressPage.SetText('Checking MongoDB Shell (mongosh)...', 'Detecting existing installation...');
  ProgressPage.SetProgress(71, 100);

  // Check if mongosh is already installed
  if FileExists('C:\Program Files\mongosh\bin\mongosh.exe') or 
     FileExists('C:\Program Files\MongoDB\mongosh\mongosh.exe') or
     FileExists('C:\Users\' + ExpandConstant('{username}') + '\AppData\Local\Programs\mongosh\mongosh.exe') then
  begin
    ProgressPage.SetText('MongoDB Shell (mongosh) already installed.', 'Skipping installation...');
    ProgressPage.SetProgress(73, 100);
    Sleep(800);
    Exit;
  end;

  ProgressPage.SetText('Installing MongoDB Shell (mongosh)...', 'Please wait while mongosh is being installed...');
  ProgressPage.SetProgress(72, 100);

  InstallerPath := ExpandConstant('{tmp}\{#MongoShellFile}');

  // Copy the MSI from your bundled files
  if not CopyLocalFile('{#MongoShellFile}', InstallerPath) then
  begin
    MsgBox('Failed to copy mongosh installer from Dependencies folder.', mbError, MB_OK);
    Result := False;
    Exit;
  end;

  // Install mongosh
  if not Exec('msiexec.exe', Format('/i "%s" /qn /norestart /l*v "%s"', [InstallerPath, LogFile]),
              '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    MsgBox('MongoDB Shell (mongosh) installation failed to execute.', mbError, MB_OK);
    Result := False;
    Exit;
  end;

  if ResultCode <> 0 then
  begin
    MsgBox('MongoDB Shell installation failed with exit code: ' + IntToStr(ResultCode) + #13#10 +
           'Check the log file at:' + #13#10 + LogFile, mbError, MB_OK);
    Result := False;
    Exit;
  end;

  // Add mongosh to PATH if needed
   if FileExists('C:\Program Files\mongosh\bin\mongosh.exe') or 
      FileExists('C:\Program Files\MongoDB\mongosh\mongosh.exe') or
      FileExists('C:\Users\' + ExpandConstant('{username}') + '\AppData\Local\Programs\mongosh\mongosh.exe') then
  begin
    MongoShellPath :=  FindMongoShell;
    if not IsPathInEnvironment(MongoShellPath) then
    begin
      ProgressPage.SetText('Adding mongosh to PATH...', 'Updating environment variables...');
      AddToSystemPath(MongoShellPath);
    end;
  end;

  ProgressPage.SetText('MongoDB Shell installation completed.', 'mongosh is ready to use.');
  ProgressPage.SetProgress(73, 100);
end;

function InstallMongoTools: Boolean;
var
  ResultCode: Integer;
  InstallerPath: String;
  MongoToolsVersion: String;
  MongoshBasePath: String;
  MongoshBinPath: String;
  LogFile: String;
begin
  Result := True;
  if not MongoToolsCheckBox.Checked then Exit;

  if SelectedMongoOption = 3 then Exit;

  // Use 64-bit Program Files for a 64-bit MSI
  MongoToolsVersion := '100';
  MongoshBasePath := ExpandConstant('{pf}\MongoDB\Tools\' + MongoToolsVersion);
  MongoshBinPath := MongoshBasePath + '\bin';
  LogFile := ExpandConstant('{tmp}\mongotools_install.log');

  ProgressPage.SetText('Checking MongoDB Tools...', 'Detecting existing installation...');
  ProgressPage.SetProgress(55, 100);

  Log('Checking MongoDB Tools...');

  // If already installed, just ensure PATH
  if IsMongoToolsInstalled then
  begin
   Log('MongoDB Tools already installed.');
    ProgressPage.SetText('MongoDB Tools already installed.', 'Checking PATH environment...');
    ProgressPage.SetProgress(65, 100);
    //MsgBox('MongoDB Tools already installed' + MongoshBinPath, mbInformation, MB_OK);

    if not IsPathInEnvironment(MongoshBinPath) then
    begin
      ProgressPage.SetText('Adding MongoDB Tools to PATH...', 'Updating environment variables...');
      if AddToSystemPath(MongoshBinPath) then
        ProgressPage.SetText('MongoDB Tools PATH updated.', 'Environment variables updated successfully.')
      else
        MsgBox('Warning: Failed to add PATH: ' + MongoshBinPath, mbInformation, MB_OK);
    end;

    ProgressPage.SetProgress(70, 100);
    Sleep(800);
    Exit;
  end;

  ProgressPage.SetText('Installing MongoDB Tools...', 'Please wait while MongoDB Tools are being installed...');
  ProgressPage.SetProgress(60, 100);

  InstallerPath := ExpandConstant('{tmp}\{#MongoToolsFile}');

  // Copy the MSI from your bundled files
  if not CopyLocalFile('{#MongoToolsFile}', InstallerPath) then
  begin
    MsgBox('Failed to copy MongoDB Tools installer from Dependencies folder.', mbError, MB_OK);
    Result := False;
    Exit;
  end;

  // Let the MSI decide the versioned path (no INSTALLLOCATION/INSTALLDIR override)
  if not Exec('msiexec.exe', Format('/i "%s" /qn ALLUSERS=1 /l*v "%s"', [InstallerPath, LogFile]),
              '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    MsgBox('MongoDB Tools installation failed to execute.', mbError, MB_OK);
    Result := False;
    Exit;
  end;

  if ResultCode = 1603 then
  begin
    MsgBox('MongoDB Tools installation failed with exit code: 1603.' + #13#10 +
           'Check the log file at:' + #13#10 + LogFile, mbError, MB_OK);
    Result := False;
    Exit;
  end;

  // Add default versioned bin to PATH
  ProgressPage.SetText('Configuring environment...', 'Adding MongoDB Tools to system PATH...');
  if AddToSystemPath(MongoshBinPath) then
    ProgressPage.SetText('MongoDB Tools installation completed.', 'Added to system PATH successfully.')
  else
    MsgBox('MongoDB Tools installed, but could not add to system PATH: ' + MongoshBinPath, mbInformation, MB_OK);

  ProgressPage.SetProgress(70, 100);
end;

procedure UninstallMongoCompass;
var
  Cmd: string;
  ResultCode: Integer;
begin
  Cmd := GetUninstallCommand('MongoDB Compass');
  if Cmd <> '' then
  begin
    Log('Uninstalling MongoDB Compass...');
    // Remove quotes and '--uninstall' if present
      if Pos('--uninstall', Cmd) > 0 then
  Delete(Cmd, Pos('--uninstall', Cmd), Length('--uninstall'));

while Pos('"', Cmd) > 0 do
  Delete(Cmd, Pos('"', Cmd), 1);

    if not Exec(Cmd, '--uninstall', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
      //MsgBox('Failed to start MongoDB Compass uninstall.', mbError, MB_OK)
    else
      //MsgBox('MongoDB Compass has been uninstalled.', mbInformation, MB_OK);
  end
  else
  begin
    Log('MongoDB Compass not found.');
  end;
end;


function ConfigureMongoDB: Boolean;
var
  ResultCode: Integer;
  MongoConfigPath: String;
  ConfigContent: String;
  MongoDBFolderVersion: String;
  MongoDBDataPath: String;
begin
  Result := True;
  
  MongoDBFolderVersion := GetMajorMinorVersion('{#MongoDBVersion}');

  ProgressPage.SetText('Configuring MongoDB...', 'Setting up MongoDB configuration...');
  ProgressPage.SetProgress(60, 100);
  
  // Determine config file path based on installation option
  if SelectedMongoOption = 2 then
  begin
     MongoConfigPath := CustomMongoPath;
    if Copy(MongoConfigPath, Length(MongoConfigPath), 1) <> '\' then
      // Build MongoDB base folder
       MongoConfigPath :=  MongoConfigPath + 'MongoDB\Server\' + MongoDBFolderVersion + '\';
    MongoDBDataPath := MongoConfigPath + 'data';
    //MongoConfigPath := MongoConfigPath + 'bin';
    MongoConfigPath := MongoConfigPath + 'log\mongod.cfg' 
  end  
  else
  begin
    MongoConfigPath := 'C:\Program Files\MongoDB\Server\'+MongoDBFolderVersion+'\';
    MongoDBDataPath := MongoConfigPath + 'data';
    MongoConfigPath := MongoConfigPath + 'log\mongod.cfg';
  end;  
  
  Log('MongoDB config file path: ' + MongoConfigPath);
  
  // Configure MongoDB with replica set
  ConfigContent := 'systemLog:' + #13#10 +
                  '  destination: file' + #13#10 +
                  '  path: '+MongoConfigPath+'' + #13#10 +
                  'storage:' + #13#10 +
                  '  dbPath: '+MongoDBDataPath+'' + #13#10 +
                  'net:' + #13#10 +
                  '  port: {#MongoPort}' + #13#10 +
                  '  bindIp: {#MongoBindIP}' + #13#10 +
                  'replication:' + #13#10 +
                  '  replSetName: "{#MongoReplicaSetName}"';
  
  if not SaveStringToFile(MongoConfigPath, ConfigContent, False) then
  begin
    Log('Failed to write MongoDB configuration file');
    Result := False;
    Exit;
  end;
  
  Log('MongoDB configuration file created successfully');
  
  // Restart MongoDB service to apply configuration
  SafeExec('net', 'stop MongoDB', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Sleep(2000);
  
  if not SafeExec('net', 'start MongoDB', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Log('Failed to restart MongoDB service');
    MsgBox('Failed to restart MongoDB service. Please restart it manually.', mbError, MB_OK);
    Result := False;
  end
  else
  begin
    Log('MongoDB service restarted successfully');
  end;
end;

function CopyPublishFiles: Boolean;
var
  InstallerDir, SourceDir, DestDir: String;
  ResultCode: Integer;
  CopyScript, ScriptFile, LogFile: String;
begin
  Result := True;
  InstallerDir := ExtractFilePath(ExpandConstant('{srcexe}'));
  SourceDir := InstallerDir + 'Publish';
  DestDir := ExpandConstant('{app}\Publish');
  LogFile := ExpandConstant('{tmp}\Copy_Files_Log.txt');
  ScriptFile := ExpandConstant('{tmp}\Copy_Files_Script.ps1');
  
  if not DirExists(SourceDir) then
  begin
    MsgBox('Source Publish folder not found: ' + SourceDir, mbError, MB_OK);
    Result := False;
    Exit;
  end;
  
  
  
  CopyScript :=
    '$ErrorActionPreference = "Continue"' + #13#10 +
    '$LogFile = "' + LogFile + '"' + #13#10 +
    '$SourceDir = "' + SourceDir + '"' + #13#10 +
    '$DestDir = "' + DestDir + '"' + #13#10 +
    'function Write-Log { param([string]$msg) "$((Get-Date) -f ''yyyy-MM-dd HH:mm:ss'') - $msg" | Out-File -FilePath $LogFile -Append -Encoding UTF8 }' + #13#10 +
    '' + #13#10 +
    'function Wait-ForProcessExit {' + #13#10 +
    '    param([string]$ProcessName, [int]$TimeoutSeconds = 30)' + #13#10 +
    '    $timeout = (Get-Date).AddSeconds($TimeoutSeconds)' + #13#10 +
    '    do {' + #13#10 +
    '        $processes = Get-Process -Name $ProcessName -ErrorAction SilentlyContinue' + #13#10 +
    '        if (-not $processes) { return $true }' + #13#10 +
    '        Start-Sleep -Seconds 1' + #13#10 +
    '    } while ((Get-Date) -lt $timeout)' + #13#10 +
    '    return $false' + #13#10 +
    '}' + #13#10 +
    '' + #13#10 +
    'function Test-AppPoolExists {' + #13#10 +
    '    param([string]$AppPoolName)' + #13#10 +
    '    try {' + #13#10 +
    '        $result = & "$env:WINDIR\System32\inetsrv\appcmd.exe" list apppool "$AppPoolName" 2>$null' + #13#10 +
    '        return ($result -and $result.Length -gt 0)' + #13#10 +
    '    } catch {' + #13#10 +
    '        return $false' + #13#10 +
    '    }' + #13#10 +
    '}' + #13#10 +
    '' + #13#10 +
    'function Get-AppPoolStatus {' + #13#10 +
    '    param([string]$AppPoolName)' + #13#10 +
    '    try {' + #13#10 +
    '        $result = & "$env:WINDIR\System32\inetsrv\appcmd.exe" list apppool "$AppPoolName" /text:state 2>$null' + #13#10 +
    '        return $result.Trim()' + #13#10 +
    '    } catch {' + #13#10 +
    '        return "Unknown"' + #13#10 +
    '    }' + #13#10 +
    '}' + #13#10 +
    '' + #13#10 +
    'function Stop-AppPoolSafely {' + #13#10 +
    '    param([string]$AppPoolName)' + #13#10 +
    '    Write-Log "Attempting to stop app pool: $AppPoolName"' + #13#10 +
    '    ' + #13#10 +
    '    $status = Get-AppPoolStatus $AppPoolName' + #13#10 +
    '    Write-Log "Current app pool status: $status"' + #13#10 +
    '    ' + #13#10 +
    '    if ($status -eq "Started") {' + #13#10 +
    '        try {' + #13#10 +
    '            # Method 1: Use appcmd' + #13#10 +
    '            Write-Log "Stopping app pool using appcmd..."' + #13#10 +
    '            & "$env:WINDIR\System32\inetsrv\appcmd.exe" stop apppool "$AppPoolName" 2>$null' + #13#10 +
    '            Start-Sleep -Seconds 3' + #13#10 +
    '            ' + #13#10 +
    '            # Check if stopped' + #13#10 +
    '            $newStatus = Get-AppPoolStatus $AppPoolName' + #13#10 +
    '            if ($newStatus -eq "Stopped") {' + #13#10 +
    '                Write-Log "App pool stopped successfully using appcmd"' + #13#10 +
    '                return $true' + #13#10 +
    '            }' + #13#10 +
    '        } catch {' + #13#10 +
    '            Write-Log "Appcmd stop failed: $($_.Exception.Message)"' + #13#10 +
    '        }' + #13#10 +
    '        ' + #13#10 +
    '        # Method 2: Kill worker processes if appcmd fails' + #13#10 +
    '        try {' + #13#10 +
    '            Write-Log "Attempting to terminate worker processes..."' + #13#10 +
    '            $workerProcesses = Get-Process -Name "w3wp" -ErrorAction SilentlyContinue' + #13#10 +
    '            foreach ($process in $workerProcesses) {' + #13#10 +
    '                Write-Log "Terminating worker process ID: $($process.Id)"' + #13#10 +
    '                $process.Kill()' + #13#10 +
    '            }' + #13#10 +
    '            Start-Sleep -Seconds 2' + #13#10 +
    '            Write-Log "Worker processes terminated"' + #13#10 +
    '            return $true' + #13#10 +
    '        } catch {' + #13#10 +
    '            Write-Log "Failed to terminate worker processes: $($_.Exception.Message)"' + #13#10 +
    '        }' + #13#10 +
    '    } else {' + #13#10 +
    '        Write-Log "App pool is already stopped (Status: $status)"' + #13#10 +
    '        return $true' + #13#10 +
    '    }' + #13#10 +
    '    return $false' + #13#10 +
    '}' + #13#10 +
    '' + #13#10 +
    'function Start-AppPoolSafely {' + #13#10 +
    '    param([string]$AppPoolName)' + #13#10 +
    '    Write-Log "Attempting to start app pool: $AppPoolName"' + #13#10 +
    '    ' + #13#10 +
    '    $status = Get-AppPoolStatus $AppPoolName' + #13#10 +
    '    Write-Log "Current app pool status: $status"' + #13#10 +
    '    ' + #13#10 +
    '    if ($status -ne "Started") {' + #13#10 +
    '        try {' + #13#10 +
    '            Write-Log "Starting app pool using appcmd..."' + #13#10 +
    '            & "$env:WINDIR\System32\inetsrv\appcmd.exe" start apppool "$AppPoolName" 2>$null' + #13#10 +
    '            Start-Sleep -Seconds 5' + #13#10 +
    '            ' + #13#10 +
    '            # Check if started' + #13#10 +
    '            $newStatus = Get-AppPoolStatus $AppPoolName' + #13#10 +
    '            if ($newStatus -eq "Started") {' + #13#10 +
    '                Write-Log "App pool started successfully"' + #13#10 +
    '                return $true' + #13#10 +
    '            } else {' + #13#10 +
    '                Write-Log "App pool status after start attempt: $newStatus"' + #13#10 +
    '                return $false' + #13#10 +
    '            }' + #13#10 +
    '        } catch {' + #13#10 +
    '            Write-Log "Failed to start app pool: $($_.Exception.Message)"' + #13#10 +
    '            return $false' + #13#10 +
    '        }' + #13#10 +
    '    } else {' + #13#10 +
    '        Write-Log "App pool is already started"' + #13#10 +
    '        return $true' + #13#10 +
    '    }' + #13#10 +
    '}' + #13#10 +
    '' + #13#10 +
    'try {' + #13#10 +
    '    Write-Log "=== Starting deployment process ==="' + #13#10 +
    '    Write-Log "Source: $SourceDir"' + #13#10 +
    '    Write-Log "Destination: $DestDir"' + #13#10 +
    '    ' + #13#10 +
    '    # Check if app pool exists using appcmd only' + #13#10 +
    '    $appPoolExists = Test-AppPoolExists "MyAppPool"' + #13#10 +
    '    Write-Log "App Pool ''MyAppPool'' exists: $appPoolExists"' + #13#10 +
    '    ' + #13#10 +
    '    # Stop app pool if it exists' + #13#10 +
    '    if ($appPoolExists) {' + #13#10 +
    '        $stopResult = Stop-AppPoolSafely "MyAppPool"' + #13#10 +
    '        if (-not $stopResult) {' + #13#10 +
    '            Write-Log "Warning: Could not properly stop app pool, but continuing with deployment"' + #13#10 +
    '        }' + #13#10 +
    '    } else {' + #13#10 +
    '        Write-Log "App Pool ''MyAppPool'' does not exist. Skipping app pool management."' + #13#10 +
    '    }' + #13#10 +
    '    ' + #13#10 +
    '    # Create app_offline.htm to safely stop the application' + #13#10 +
    '    $appOfflineFile = Join-Path $DestDir "app_offline.htm"' + #13#10 +
    '    try {' + #13#10 +
    '        if (Test-Path $DestDir) {' + #13#10 +
    '            Write-Log "Creating app_offline.htm for safe deployment"' + #13#10 +
    '            $offlineContent = "<html><head><title>Application Offline</title></head><body><h1>Application is being updated...</h1><p>Please try again in a few minutes.</p></body></html>"' + #13#10 +
    '            $offlineContent | Out-File -FilePath $appOfflineFile -Encoding UTF8' + #13#10 +
    '            Start-Sleep -Seconds 2' + #13#10 +
    '        }' + #13#10 +
    '    } catch {' + #13#10 +
    '        Write-Log "Could not create app_offline.htm: $($_.Exception.Message)"' + #13#10 +
    '    }' + #13#10 +
    '    ' + #13#10 +
    '    # Perform file copy' + #13#10 +
    '    Write-Log "Starting file copy operation..."' + #13#10 +
    '    $params = @(' + #13#10 +
    '        "$SourceDir", "$DestDir",' + #13#10 +
    '        "/MIR", "/R:5", "/W:3", "/NP", "/NFL", "/NDL", "/LOG+:$LogFile"' + #13#10 +
    '    )' + #13#10 +
    
    ' $licenseDest = Join-Path $DestDir "License"' + #13#10 +
    ' if (Test-Path $licenseDest) {' + #13#10 +
    '  Write-Log "License folder exists. Excluding from copy."' + #13#10 +
    '  $params += "/XD"' + #13#10 +
    '  $params += "License"' + #13#10 +
    ' } else {' + #13#10 +
    '  Write-Log "License folder not found. It will be copied."' + #13#10 +
    ' }' + #13#10 +
    
    '    robocopy @params' + #13#10 +
    '    $code = $LASTEXITCODE' + #13#10 +
    '    if ($code -le 7) {' + #13#10 +
    '        Write-Log "Robocopy succeeded (ExitCode=$code)"' + #13#10 +
    '    } else {' + #13#10 +
    '        Write-Log "Robocopy failed (ExitCode=$code)"' + #13#10 +
    '        exit $code' + #13#10 +
    '    }' + #13#10 +
    '    ' + #13#10 +
    '    # Remove app_offline.htm' + #13#10 +
    '    try {' + #13#10 +
    '        if (Test-Path $appOfflineFile) {' + #13#10 +
    '            Remove-Item $appOfflineFile -Force' + #13#10 +
    '            Write-Log "Removed app_offline.htm"' + #13#10 +
    '        }' + #13#10 +
    '    } catch {' + #13#10 +
    '        Write-Log "Could not remove app_offline.htm: $($_.Exception.Message)"' + #13#10 +
    '    }' + #13#10 +
    '    ' + #13#10 +
    '    # Start app pool if it exists' + #13#10 +
    '    if ($appPoolExists) {' + #13#10 +
    '        Start-Sleep -Seconds 2' + #13#10 +
    '        $startResult = Start-AppPoolSafely "MyAppPool"' + #13#10 +
    '        if (-not $startResult) {' + #13#10 +
    '            Write-Log "Warning: Could not start app pool automatically."' + #13#10 +
    '            Write-Log "You may need to manually start the app pool ''MyAppPool'' in IIS Manager."' + #13#10 +
    '        }' + #13#10 +
    '    }' + #13#10 +
    '    ' + #13#10 +
    '    Write-Log "=== Deployment process completed ==="' + #13#10 +
    '    exit 0' + #13#10 +
    '} catch {' + #13#10 +
    '    Write-Log "Critical exception occurred: $($_.Exception.Message)"' + #13#10 +
    '    Write-Log "Stack trace: $($_.ScriptStackTrace)"' + #13#10 +
    '    exit 1' + #13#10 +
    '}';
  
  if not SaveStringToFile(ScriptFile, CopyScript, False) then
  begin
    MsgBox('Failed to create PowerShell copy script.', mbError, MB_OK);
    Result := False;
    Exit;
  end;
  
  if not Exec('powershell.exe', '-ExecutionPolicy Bypass -File "' + ScriptFile + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    MsgBox('PowerShell script execution failed.', mbError, MB_OK);
    Result := False;
    Exit;
  end;
  
  if ResultCode <> 0 then
  begin
    MsgBox('File copy failed (Exit Code: ' + IntToStr(ResultCode) + '). Check log file: ' + LogFile, mbError, MB_OK);
    Result := False;
    Exit;
  end;
end;

// Apply MongoDB configuration
procedure ApplyMongoConfiguration;
var
  EnvVarName: String;
  DefaultConnString: String;
  FinalConnString: String;
begin
  //EnvVarName := 'VisionInsightMongoConn';
  EnvVarName := '{#MongoEnvVarName}';
  //DefaultConnString := 'mongodb://localhost:27017';
  DefaultConnString := '{#MongoDefaultConnString}';
  
  // Check if environment variable exists
  if not EnvironmentVariableExists(EnvVarName) then
  begin
    Log('Environment variable does not exist. Creating: ' + EnvVarName);
    SetEnvironmentVariable(EnvVarName, DefaultConnString);
  end;
  
  // Apply configuration based on selected option
  case SelectedMongoOption of
    1: // Default
      begin
        Log('Using default MongoDB configuration');
        FinalConnString := DefaultConnString;
        Log('FinalConnString '+FinalConnString);
        SetEnvironmentVariable(EnvVarName, FinalConnString);
        Log('SetEnvironmentVariable ');
      end;
      
    2: // Custom Path
      begin
        Log('Using custom MongoDB data path: ' + CustomMongoPath);
        // You can use CustomMongoPath variable for MongoDB configuration
        // For now, we'll keep the default connection string but log the path
        FinalConnString := DefaultConnString;
        SetEnvironmentVariable(EnvVarName, FinalConnString);
        
        // Store custom path in a separate environment variable if needed
        //SetEnvironmentVariable('VisionInsightMongoDataPath', CustomMongoPath);
      end;
      
    3: // Connection String
      begin
        Log('Using custom connection string: ' + MongoConnectionString);
        Log('DefaultConnString connection string: ' + DefaultConnString);
        FinalConnString := MongoConnectionString;
        SetEnvironmentVariable(EnvVarName, FinalConnString);
      end;
  end;
  
  Log('MongoDB configuration applied. Connection string: ' + FinalConnString);
end;


function CreateDatabase: Boolean;
var
  ResultCode: Integer;
  DatabaseScript: String;
  ScriptPath: String;
  MongoShellPath: String;
  InstallerPath: string;
begin
  Result := True;
  if not (MongoDBCheckBox.Checked and MongoToolsCheckBox.Checked) then Exit;
  
  if SelectedMongoOption = 3 then Exit;

  try
    ProgressPage.SetText('Creating MongoDB Database...', 'Setting up database and collections...');
    ProgressPage.SetProgress(98, 100);
    
    ScriptPath := ExpandConstant('{tmp}\create_db.js');
    MongoShellPath :=  FindMongoShell;
    
    // Check if mongosh exists
    if not FileExists(MongoShellPath) then
    begin
      ProgressPage.SetText('mongosh not found...', 'Installing mongosh from local MSI...');
      ProgressPage.SetProgress(96, 100);

      // Use the correct path - same as other installers in your code
      InstallerPath := ExpandConstant('{tmp}\{#MongoShellFile}');
      
      // Copy local file to temp 
      if not CopyLocalFile('{#MongoShellFile}', InstallerPath) then
      begin
        MsgBox('Failed to copy mongosh installer from Dependencies folder.', mbError, MB_OK);
        Result := False;
        Exit;
      end;

      // Install mongosh silently
      if not Exec('msiexec.exe', '/i "' + InstallerPath + '" /qn /norestart', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
      begin
        MsgBox('Failed to execute mongosh installer. Error code: ' + IntToStr(ResultCode), mbError, MB_OK);
        Result := False;
        Exit;
      end;
      
      if ResultCode <> 0 then
      begin
        MsgBox('mongosh installation failed with exit code: ' + IntToStr(ResultCode), mbError, MB_OK);
        Result := False;
        Exit;
      end;

      Sleep(3000); // Wait for installation to complete

      // Verify installation
      if not FileExists(MongoShellPath) then
      begin
        MsgBox('mongosh installation completed but executable not found at: ' + MongoShellPath, mbError, MB_OK);
        Result := False;
        Exit;
      end;
      
      ProgressPage.SetText('mongosh installed successfully.', 'Proceeding with database creation...');
    end;
    
    DatabaseScript :=
      'print("Starting database creation...");' + #13#10 +
      'const dbName = {#MongoDBName};' + #13#10 +
      'const collectionName = "Test";' + #13#10 +
      'const dbs = db.getMongo().getDBNames();' + #13#10 +
      'if (dbs.includes(dbName)) {' + #13#10 +
      '  print(`Database ${dbName} already exists.`);' + #13#10 +
      '  use(dbName);' + #13#10 +
      '  const collections = db.getCollectionNames();' + #13#10 +
      '  if (collections.includes(collectionName)) {' + #13#10 +
      '    print(`Collection ${collectionName} already exists. Skipping creation.`);' + #13#10 +
      '  } else {' + #13#10 +
      '    db.createCollection(collectionName);' + #13#10 +
      '    print(`Collection ${collectionName} created successfully.`);' + #13#10 +
      '  }' + #13#10 +
      '} else {' + #13#10 +
      '  print(`Database ${dbName} does not exist. Creating now...`);' + #13#10 +
      '  use(dbName);' + #13#10 +
      '  db.createCollection(collectionName);' + #13#10 +
      '  print(`Database ${dbName} and collection ${collectionName} created successfully.`);' + #13#10 +
      '}';
    
    SaveStringToFile(ScriptPath, DatabaseScript, False);
    
    Sleep(3000); // Wait for MongoDB service to be ready
    
    if not SafeExec(MongoShellPath, 'mongodb://localhost:27017/ --file "' + ScriptPath + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
    begin
      // Don't fail the installation for database creation issues
      ProgressPage.SetText('Database creation completed with warnings.', 'You may need to create the database manually.');
    end;
    
    // Clean up script file
    if FileExists(ScriptPath) then
      DeleteFile(ScriptPath);
      
    ProgressPage.SetProgress(100, 100);
  except
    begin
      ProgressPage.SetText('Database creation completed with warnings.', 'You may need to create the database manually.');
    end;
  end;
end;

procedure CreateScheduledTask();
var
  ResultCode: Integer;
  ScriptPath: string;
  PSScript: TStringList;
  TempScriptPath: string;
begin
  ScriptPath := ExpandConstant('{app}\{#DepsFolder}\testrunningapp.ps1');
  TempScriptPath := ExpandConstant('{tmp}\CreateTask.ps1');
  
  // Create a temporary PowerShell script file
  PSScript := TStringList.Create;
  try
    PSScript.Add('$TaskName = "HanwhaVision"');
    PSScript.Add('$ScriptPath = "' + ScriptPath + '"');
    PSScript.Add('');
    PSScript.Add('$TaskAction = New-ScheduledTaskAction -Execute "powershell.exe" -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$ScriptPath`""');
    PSScript.Add('');
    PSScript.Add('$TaskTrigger = New-ScheduledTaskTrigger -Once -At (Get-Date) -RepetitionInterval (New-TimeSpan -Minutes 5)');
    PSScript.Add('');
    PSScript.Add('$TaskPrincipal = New-ScheduledTaskPrincipal -UserId "NT AUTHORITY\SYSTEM" -RunLevel Highest');
    PSScript.Add('');
    PSScript.Add('Register-ScheduledTask -TaskName $TaskName -Action $TaskAction -Trigger $TaskTrigger -Principal $TaskPrincipal -Force');
    PSScript.Add('');
    PSScript.Add('Start-ScheduledTask -TaskName $TaskName');
    
    PSScript.SaveToFile(TempScriptPath);
  finally
    PSScript.Free;
  end;
  
  // Execute the temporary PowerShell script
  if Exec('powershell.exe', 
    '-NoProfile -ExecutionPolicy Bypass -File "' + TempScriptPath + '"',
    '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    if ResultCode = 0 then
      Log('Scheduled task created and started successfully')
    else
      MsgBox('Failed to create scheduled task. Error code: ' + IntToStr(ResultCode), mbError, MB_OK);
  end
  else
    MsgBox('Failed to execute PowerShell command', mbError, MB_OK);
    
  // Clean up temp file
  DeleteFile(TempScriptPath);
end;

function CreateReplicaSetScriptFile(MongoShellPath: String): String;
var
  ScriptPath: String;
  ScriptContent: String;
begin
  ScriptPath := ExpandConstant('{tmp}') + '\init_replica.ps1';
  
  ScriptContent := '# MongoDB Replica Set Initialization Script' + #13#10 +
                   '$ErrorActionPreference = "Stop"' + #13#10 +
                   '' + #13#10 +
                   'try {' + #13#10 +
                   '    $tempJs = "$env:TEMP\mongo_init.js"' + #13#10 +
                   '    $script = @"' + #13#10 +
                   'rs.initiate({ _id: "rs0", members: [{ _id: 0, host: "localhost:27017" }] })' + #13#10 +
                   '"@' + #13#10 +
                   '' + #13#10 +
                   '    Set-Content -Path $tempJs -Value $script' + #13#10 +
                   '    Write-Host "Executing mongosh with script file..."' + #13#10 +
                   '    ' + #13#10 +
                   '    $result = & "' + MongoShellPath + '" $tempJs 2>&1 | Out-String' + #13#10 +
                   '    Write-Host "Result: $result"' + #13#10 +
                   '    ' + #13#10 +
                   '    Remove-Item $tempJs -Force -ErrorAction SilentlyContinue' + #13#10 +
                   '    ' + #13#10 +
                   '    if ($result -match "ok.*1") {' + #13#10 +
                   '        Write-Host "Replica set initialized successfully"' + #13#10 +
                   '        exit 0' + #13#10 +
                   '    }' + #13#10 +
                   '    elseif ($result -match "already initialized") {' + #13#10 +
                   '        Write-Host "Replica set already initialized"' + #13#10 +
                   '        exit 0' + #13#10 +
                   '    }' + #13#10 +
                   '    else {' + #13#10 +
                   '        Write-Host "Unexpected result"' + #13#10 +
                   '        exit 1' + #13#10 +
                   '    }' + #13#10 +
                   '}' + #13#10 +
                   'catch {' + #13#10 +
                   '    Write-Host "Error: $_"' + #13#10 +
                   '    exit 1' + #13#10 +
                   '}';
  
  SaveStringToFile(ScriptPath, ScriptContent, False);
  Result := ScriptPath;
end;

function CreateReplicaSet: Boolean;
var
  ResultCode: Integer;
  UpdateConfigCmd, InitReplicaCmd, CheckReplicaCmd: String;
  MongoShellPath: String;
  ConfigFilePath: String;
  MongoDBFolderVersion: String;
begin
  Result := False;

  if SelectedMongoOption = 3 then
  begin
    Log('Using connection string - skipping replica set initialization');
    Result := True;
    Exit;
  end;

  Log('--- MongoDB Replica Set Setup Started ---');

  // Find mongosh executable
  MongoShellPath := FindMongoShell;
  
  if MongoShellPath = '' then
  begin
    Log('mongosh not found - cannot create replica set');
    MsgBox('MongoDB Shell (mongosh) was not found.' + #13#10 +
           'Replica set creation will be skipped.' + #13#10 +
           'You can create the replica set manually after installation.', 
           mbInformation, MB_OK);
    Result := True; // Don't fail installation
    Exit;
  end;
  
  Log('Using mongosh at: ' + MongoShellPath);

  MongoDBFolderVersion := GetMajorMinorVersion('{#MongoDBVersion}');

  // Determine MongoDB installation path based on user selection
  if SelectedMongoOption = 2 then
  begin
    ConfigFilePath := CustomMongoPath;
    if Copy(ConfigFilePath, Length(ConfigFilePath), 1) <> '\' then
      ConfigFilePath := ConfigFilePath + '\MongoDB\Server\'+MongoDBFolderVersion+'\bin\';
    ConfigFilePath := ConfigFilePath + 'mongod.cfg';
  end
  else
  begin
    ConfigFilePath := 'C:\Program Files\MongoDB\Server\'+MongoDBFolderVersion+'\bin\mongod.cfg';
  end;

  Log('Config file path: ' + ConfigFilePath);

  { STEP 1: Add replication settings to mongod.cfg if not already present. }
  UpdateConfigCmd := '$conf = ''' + ConfigFilePath + '''; ' +
                     'if (Test-Path $conf) { ' +
                     '  $txt = Get-Content $conf -Raw; ' +
                     '  if ($txt -notmatch ''replSetName'') { ' +
                     '    Add-Content -Path $conf -Value @('''', ''replication:'', ''  replSetName: rs0''); ' +
                     '    Write-Host "Replication config added"; ' +
                     '  } else { ' +
                     '    Write-Host "Replication already configured"; ' +
                     '  } ' +
                     '} else { ' +
                     '  Write-Host "Config file not found: $conf"; ' +
                     '  exit 1; ' +
                     '}';

  if not SafeExec('powershell.exe', '-NoProfile -ExecutionPolicy Bypass -Command "' + UpdateConfigCmd + '"',
                  '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Log('Failed: Updating config file. Exit code: ' + IntToStr(ResultCode));
    MsgBox('Failed to update MongoDB configuration file.' + #13#10 +
           'You may need to manually add replica set configuration.', mbInformation, MB_OK);
    Result := True; // Don't fail installation
    Exit;
  end;

  { STEP 2: Restart MongoDB Service }
  Log('Stopping MongoDB service...');
  if not SafeExec('cmd.exe', '/c net stop MongoDB', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Log('Failed: Stopping MongoDB. Exit code: ' + IntToStr(ResultCode));
    // Continue anyway - service might not be running
  end;

  Sleep(2000); // Wait for service to stop

  Log('Starting MongoDB service...');
  if not SafeExec('cmd.exe', '/c net start MongoDB', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Log('Failed: Starting MongoDB. Exit code: ' + IntToStr(ResultCode));
    MsgBox('Failed to restart MongoDB service.' + #13#10 +
           'Please restart the service manually and run replica set initialization.', mbError, MB_OK);
    Result := False;
    Exit;
  end;

  Sleep(5000); // Wait for MongoDB to be ready

  { STEP 3: Verify MongoDB service is up and running }
  Log('Verifying MongoDB service is operational...');
  
  if not IsMongoDBServiceRunning(MongoShellPath) then
  begin
    Log('MongoDB service is not responding. Waiting additional 5 seconds...');
    Sleep(5000);
    
    // Try one more time
    if not IsMongoDBServiceRunning(MongoShellPath) then
    begin
      Log('MongoDB service is still not responding after retry');
      MsgBox('MongoDB service is not responding.' + #13#10 +
             'Please check if MongoDB service is running and try again.' + #13#10 +
             'You can manually initialize replica set later using:' + #13#10 +
             'rs.initiate({ _id: "rs0", members: [{ _id: 0, host: "localhost:27017" }] })', 
             mbError, MB_OK);
      Result := True; // Don't fail installation
      Exit;
    end;
  end;
  
  Log('MongoDB service is confirmed running and responding to commands');

  { STEP 4: Check if Replica Set is already initialized }

  CheckReplicaCmd := 'Start-Sleep -Seconds 2; ' +
                   'try { ' +
                   '  $result = & "' + MongoShellPath + '" --quiet --eval "rs.status().ok" 2>&1; ' +
                   '  Write-Host "Check result: $result"; ' +
                   '  if ($result -match "1") { Write-Host "Already initialized"; exit 0 } else { Write-Host "Not initialized"; exit 1 } ' +
                   '} catch { Write-Host "Check failed: $_"; exit 1 }';               

  Log('Checking if replica set is already initialized...');
  Log('CheckReplicaCmd = ' + CheckReplicaCmd);
  SafeExec('powershell.exe', '-NoProfile -ExecutionPolicy Bypass -Command "' + CheckReplicaCmd + '"',
           '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  
  Log('Check ResultCode: ' + IntToStr(ResultCode));

  if ResultCode = 0 then
  begin
    Log('Replica Set already initialized. Skipping initialization.');
    Result := True;
    Exit;
  end;

  { STEP 5: Initialize Replica Set (only if not already initialized) }
  Log('Initializing replica set...');

  // Create PowerShell script file instead of inline command
  InitReplicaCmd :=  CreateReplicaSetScriptFile(MongoShellPath);

  Log('InitReplicaCmd = ' + InitReplicaCmd);


  if not SafeExec('powershell.exe', '-NoProfile -ExecutionPolicy Bypass -Command "' + InitReplicaCmd + '"',
                  '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Log('Replica initialization failed to execute. Exit code: ' + IntToStr(ResultCode));
    MsgBox('Failed to initialize MongoDB replica set.' + #13#10 +
           'You can manually run this command in mongosh:' + #13#10 +
           'rs.initiate({ _id: "rs0", members: [{ _id: 0, host: "localhost:27017" }] })', 
           mbInformation, MB_OK);
    Result := True; // Don't fail installation
    Exit;
  end;

  if ResultCode <> 0 then
  begin
    Log('Replica initialization returned non-zero exit code: ' + IntToStr(ResultCode));
   // MsgBox('Replica set initialization may have failed.' + #13#10 +
   //        'Check MongoDB logs or run manually:' + #13#10 +
   //        'rs.initiate({ _id: "rs0", members: [{ _id: 0, host: "localhost:27017" }] })', 
   //        mbInformation, MB_OK);
    Result := True; // Don't fail installation
    Exit;
  end;

  Log('--- MongoDB Replica Set Successfully Initialized ---');
  Result := True;
end;


// Handle page changes
procedure CurPageChanged(CurPageID: Integer);
begin
 //Log('MongoConnectionString set to: ' + CurPageID);
  if CurPageID = wpFinished then
  begin
    InstallSuccessful := True; // Mark as successful when reaching finish page
    RestartCheckbox.Visible := True;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  InstallationSuccessful: Boolean;
begin
  if CurStep <> ssPostInstall then
    Exit;

  // Create progress page
  ProgressPage :=
    CreateOutputProgressPage(
      'Installing Components',
      'Please wait while the selected components are being installed...'
    );
  ProgressPage.Show;

  try
    InstallationSuccessful := True;

    ProgressPage.SetProgress(0, 100);
    ProgressPage.SetText(
      'Starting installation process...',
      'Preparing to install selected components...'
    );

    // IIS
    if not SetupIIS then
    begin
      Log('SetupIIS failed');
      InstallationSuccessful := False;
    end;

    // .NET Hosting Bundle
    if not InstallNetHostingBundle then
    begin
      Log('InstallNetHostingBundle failed');
      InstallationSuccessful := False;
    end;

    // .NET SDK
    if not InstallDotNetSDK then
    begin
      Log('InstallDotNetSDK failed');
      InstallationSuccessful := False;
    end;

    // MongoDB Server
    if not InstallMongoDB then
    begin
      Log('InstallMongoDB failed');
      InstallationSuccessful := False;
    end;

    // Mongo Tools
    if not InstallMongoTools then
    begin
      Log('InstallMongoTools failed');
      InstallationSuccessful := False;
    end;

    // Mongo Shell
    if not InstallMongoShell then
    begin
      Log('InstallMongoShell failed');
      InstallationSuccessful := False;
    end;

    // MongoDB Configuration (non-fatal)
    if not ConfigureMongoDB then
      Log('ConfigureMongoDB failed');

    // Remove Compass if tools installed
    //if InstallMongoTools then
    //  UninstallMongoCompass;

    // Copy published files after IIS setup
    if SetupIIS then
    begin
      if not CopyPublishFiles then
      begin
        MsgBox(
          'IIS was set up successfully, but file copying failed. ' +
          'You may need to manually copy files from the installation directory.',
          mbInformation,
          MB_OK
        );
      end;
    end;

    // Apply Mongo settings
    ApplyMongoConfiguration;

    // Database creation (non-fatal)
    if not CreateDatabase then
      Log('CreateDatabase failed');

    // Scheduled task
    CreateScheduledTask;

    // Replica set
    if not CreateReplicaSet then
      MsgBox('Replica Set failed. Check installation log.', mbError, MB_OK);

    // Final status
    if InstallationSuccessful then
    begin
      ProgressPage.SetText(
        'Installation completed successfully!',
        'All selected components have been installed.'
      );
    end
    else
    begin
      ProgressPage.SetText(
        'Installation completed with some issues.',
        'Some components may need manual configuration.'
      );
    end;

    Sleep(2000);

  finally
    ProgressPage.Hide;
  end;
end;

procedure DeinitializeSetup;
begin
  if InstallSuccessful and RestartCheckbox.Checked then
  begin
    if MsgBox('The system will now restart. Do you want to proceed?', mbConfirmation, MB_YESNO) = IDYES then
    begin
      Exec('shutdown', '/r /t 0', '', SW_HIDE, ewNoWait, ResultCode);
    end;
  end;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;

if (MongoConfigPage <> nil) and (CurPageID = MongoConfigPage.ID) then
  begin
  //Log('NextButtonClick come: ' + CurPageID);
    if RadioConnectionString.Checked then
  // When leaving MongoDB configuration page
    begin
      MongoConnectionString := Trim(EditConnectionString.Text);
      if MongoConnectionString = '' then
      begin
        MsgBox('Please enter MongoDB Connection String.', mbError, MB_OK);
        Result := False;  // Stay on same page
        Exit;
      end;
      Log('MongoConnectionString set to: ' + MongoConnectionString);
    end;
    if SelectedMongoOption = 2 then
      begin
          if CustomMongoPath = '' then
          begin
            MsgBox('Please select MongoDB installation directory.', mbError, MB_OK);
            Result := False;  // Stay on same page
            Exit;
          end
      end
    end;

end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  ResultCode: Integer;
begin
  if CurUninstallStep = usPostUninstall then
  begin
  if not IsMongoDBInstalled then
  begin
    // Stop and remove MongoDB service
    SafeExec('net', 'stop mongodb', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    SafeExec('sc', 'delete mongodb', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  end;  
    // Remove IIS site and app pool
    SafeExec('powershell.exe', '{#PSExecutionPolicy} -Command "try { Import-Module WebAdministration; Remove-IISSite -Name MyApp -Confirm:$false; Remove-IISAppPool -Name MyAppPool -Confirm:$false } catch { Write-Host ''Cleanup completed'' }"', 
         '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  end;
end;
