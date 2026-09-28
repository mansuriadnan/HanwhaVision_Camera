[Setup]
AppName=MongoDB Installer
AppVersion=7.0.12
DefaultDirName={autopf}\MongoDB\Server\7.0
CreateAppDir=yes
OutputDir=Output
OutputBaseFilename=MongoDB-Installer

[CustomMessages]
SelectMongoPath=Select MongoDB Installation Directory:

[Files]
Source: "{src}\Dependencies\mongodb-windows-x86_64-7.0.12-signed.msi"; DestDir: "{tmp}"; Flags: external

[Code]
var
  MongoPathPage: TInputDirWizardPage;
  MongoInstallPath: String;

procedure InitializeWizard;
begin
  // Create custom page for MongoDB path selection
  MongoPathPage := CreateInputDirPage(wpSelectDir,
    'MongoDB Installation Path',
    'Choose where MongoDB should be installed',
    CustomMessage('SelectMongoPath'),
    False, '');
  
  // Set default path
  MongoPathPage.Add('C:\Program Files\MongoDB\Server\7.0\');
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  
  if CurPageID = MongoPathPage.ID then
  begin
    MongoInstallPath := MongoPathPage.Values[0];
    // Validate path selection
    if MongoInstallPath = '' then
    begin
      MsgBox('Please select a valid installation path for MongoDB.', mbError, MB_OK);
      Result := False;
    end;
  end;
end;

procedure CreateMongoDBStructure(InstallPath: String);
var
  DataPath, LogPath, ConfigPath: String;
  ConfigContent: String;
begin
  DataPath := InstallPath + '\data\db';
  LogPath := InstallPath + '\data\log';
  ConfigPath := InstallPath + '\mongod.cfg';
  
  // Create data directories
  ForceDirectories(DataPath);
  ForceDirectories(LogPath);
  
  // Create basic configuration file
  ConfigContent := 'systemLog:' + #13#10 +
                   '  destination: "file"' + #13#10 +
                   '  path: "' + LogPath + '\mongod.log"' + #13#10 +
                   'storage:' + #13#10 +
                   '  dbPath: "' + DataPath + '"' + #13#10;
  
  SaveStringToFile(ConfigPath, ConfigContent, False);
end;


procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  MSIPath: String;
  Parameters: String;
begin
  if CurStep = ssPostInstall then
  begin
    MSIPath := ExpandConstant('{tmp}\mongodb-windows-x86_64-7.0.12-signed.msi');
    
    // Build MongoDB installation parameters
    Parameters := '/qb /i "' + MSIPath + '" ' +
                  'INSTALLLOCATION="' + MongoInstallPath + '" ' +
                  'ADDLOCAL="ServerService,Client" ' +
                  'SHOULD_INSTALL_COMPASS="0"';
    
    // Execute MongoDB MSI installer
    if not Exec('msiexec.exe', Parameters, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
    begin
      MsgBox('MongoDB installation failed. Error code: ' + IntToStr(ResultCode), mbError, MB_OK);
    end
    else
    begin
      if ResultCode = 0 then
      begin
        MsgBox('MongoDB has been successfully installed to: ' + MongoInstallPath, mbInformation, MB_OK);
        
        // Create MongoDB directories and configuration
        CreateMongoDBStructure(MongoInstallPath);
      end
      else
      begin
        MsgBox('MongoDB installation failed with exit code: ' + IntToStr(ResultCode), mbError, MB_OK);
      end;
    end;
  end;
end;

