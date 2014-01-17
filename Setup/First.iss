[Setup]
ArchitecturesInstallIn64BitMode=x64
AppName=Code Reviewer
AppVerName=Code Reviewer
AppVersion=1.1
DefaultDirName={pf}\Code Reviewer
DefaultGroupName=Code Reviewer
UninstallDisplayIcon={app}\logo.png
Compression=none
SolidCompression=true
OutputDir=C:\src\CodeReviewer\bin
SourceDir=c:\src\CodeReviewer
AppCopyright=Copyright Joy Of Playing Industries 2013
SetupLogging=true
AppPublisher=Carlos Gomes Industries
AppPublisherURL=http://www.joyofplaying.com
UninstallDisplayName=Code Reviewer Setup

[Types]
Name: full; Description: Full installation
Name: website; Description: Web site installation
Name: database; Description: Database installation
Name: tools; Description: Email/Tools installation; Flags: iscustom

[Components]
Name: website; Description: Install Code Reviewer web site; Types: full website
Name: database; Description: Install Code Reviewer database; Types: full database
Name: tools; Description: Install Email/Tools; Types: full tools

;[Tasks]
;Name: website; Description: "Install Code Reviewer web site"; Components: website;
;Name: database; Description: "Install Code Reviewer database"; Components: database;
;Name: tools; Description: "Install Email/Tools"; Components: tools;

[Files]
Source: CodeReviewer\Images\logo.png; DestDir: {app}
Source: Database\bin\Release\Database.publish.sql; DestDir: {app}\database; Components: database
Source: bin\diff.exe; DestDir: {app}\bin; Components: tools
Source: GenDiffFiles\bin\Release\*; DestDir: {app}\GenDiffFiles; Components: tools
Source: Notifier\bin\Release\*; DestDir: {app}\Notifier; Components: tools
Source: ReviewExe\bin\Release\*; DestDir: {app}\ReviewExe; Components: tools
Source: bin\CodeReviewer.deploy-readme.txt; DestDir: {app}\website; Components: website
Source: bin\CodeReviewer.deploy.cmd; DestDir: {app}\website; Components: website
Source: bin\CodeReviewer.SetParameters.xml; DestDir: {app}\website; Components: website
Source: bin\CodeReviewer.SourceManifest.xml; DestDir: {app}\website; Components: website
Source: bin\CodeReviewer.zip; DestDir: {app}\website; Components: website

[Icons]
Name: {group}\Code Reviewer; Filename: {app}\logo.png

;[Run]
;Filename: cmd.exe; Parameters: /c {app}\website\CodeReviewer.deploy.cmd /T; StatusMsg: Installing web site; Flags: 64bit runascurrentuser postinstall

[Code]
var
  MyProgCheckResult: Boolean;
  FinishedInstall: Boolean;

  WebSitePage: TInputQueryWizardPage;
  DatabasePage: TInputQueryWizardPage;
  EmailPage: TInputQueryWizardPage;
  EmailOptionsPage: TInputOptionWizardPage;

function GetPreviousBool(const Key: String; Default: Boolean): Boolean;
begin
     if (GetPreviousData(Key, 'False') = 'True') then
      Result := True
    else
      Result := False
end;

procedure SetPreviousBool(PreviousDataKey: Integer; Key: String; Value: Boolean);
var
  data: String;
begin
    data := 'False';
    if (Value = True) then
      data := 'True';

    SetPreviousData(PreviousDataKey, Key, data);
end;

procedure InitializeWizard;
begin
  { Create the pages }
  Log('InitializeSetup called');

  WebSitePage := CreateInputQueryPage(wpSelectComponents,
    'Web Site Information', 'What is the Web Server Name?',
    'Please specify the Web Server Name, along with a custom website name (optional).');
  WebSitePage.Add('Web Server name:', False);
  WebSitePage.Add('Custom web site name:', False);

  WebSitePage.Values[0] := GetPreviousData('WebSitePage.WebServerName', ExpandConstant('{computername}'));
  WebSitePage.Values[1] := GetPreviousData('WebSitePage.WebSiteName', 'CodeReviewer');

  DatabasePage := CreateInputQueryPage(WebSitePage.ID,
    'Database Server Information', 'What is the Database Server Name?',
    'Please specify the Database Server Name.');
  DatabasePage.Add('Database Server name:', False);

  DatabasePage.Values[0] := GetPreviousData('DatabasePage.DatabaseServerName', ExpandConstant('{computername}'));

  EmailOptionsPage := CreateInputOptionPage(DatabasePage.ID,
    'Email Options', 'Select SMTP and/or SSL options.  See readme for configuring Exchange server connections',
    '',
    True, False);
  EmailOptionsPage.Add('Use SMTP to send Code Reviewer notification emails');
  EmailOptionsPage.Add('Use SSL');

  EmailOptionsPage.Values[0] := GetPreviousBool('EmailOptionsPage.UseSMTP', False);
  EmailOptionsPage.Values[1] := GetPreviousBool('EmailOptionsPage.UseSSL', False);

  EmailPage := CreateInputQueryPage(EmailOptionsPage.ID,
    'Email SMTP Information', 'What is your SMTP information?',
    'Please specify the SMTP information for sending Code Reviewer notifications.');
  EmailPage.Add('SMTP Server. for example smtp.foo.com:', False);
  EmailPage.Add('User id:', False);
  EmailPage.Add('Password', True);
  EmailPage.Add('Port', False);

  EmailPage.Values[0] := GetPreviousData('EmailPage.SMTPServer', '');
  EmailPage.Values[1] := GetPreviousData('EmailPage.UserId', '');
  EmailPage.Values[3] := GetPreviousData('EmailPage.Port', '587');

end;

function ShouldSkipPage(PageID: Integer): Boolean;
begin
  { Skip pages that shouldn't be shown }
  if (PageID < WebSitePage.ID) then
       Result := True
  else if (PageID = WebSitePage.ID) then
    if (IsComponentSelected('website')) then
      Result := False
    else
      Result := True
  else if (PageID = DatabasePage.ID) then
    if (IsComponentSelected('database')) then
      Result := False
    else
      Result := True
  else if (PageID = EmailPage.ID) then
    if (IsComponentSelected('tools')) and (EmailOptionsPage.Values[0]) then
      Result := False
    else
      Result := True
  else if (PageID = EmailOptionsPage.ID) then
    if (IsComponentSelected('tools')) then
      Result := False
    else
      Result := True
  else
    Result := False;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  I: Integer;
begin
  Result := True;
  if CurPageID = WebSitePage.ID then begin
    if WebSitePage.Values[0] = '' then begin
      MsgBox('You must enter a Web Server.', mbError, MB_OK);
      Result := False;
    end
    else if WebSitePage.Values[1] = '' then begin
        MsgBox('You must enter a Web Site name.', mbError, MB_OK);
        Result := False;
    end
    else begin
      SetPreviousData(0, 'WebSitePage.WebServerName', WebSitePage.Values[0]);
      SetPreviousData(0, 'WebSitePage.WebSiteName', WebSitePage.Values[1]);
    end
  end;

  if CurPageID = DatabasePage.ID then begin
    if DatabasePage.Values[0] = '' then begin
      MsgBox('You must enter a Database Server name.', mbError, MB_OK);
      Result := False;
    end
  end;

  if CurPageID = EmailPage.ID then begin
    if EmailPage.Values[0] = '' then begin
      MsgBox('You must enter a SMTP Server name.', mbError, MB_OK);
      Result := False;
    end
    else if EmailPage.Values[1] = '' then begin
      MsgBox('You must enter a User name.', mbError, MB_OK);
      Result := False;
    end
    else if EmailPage.Values[2] = '' then begin
      MsgBox('You must enter a password.', mbError, MB_OK);
      Result := False;
    end
    else if EmailPage.Values[3] = '' then begin
      MsgBox('You must enter a port.', mbError, MB_OK);
      Result := False;
    end
    else
      Result := True;
  end;

end;

procedure RegisterPreviousData(PreviousDataKey: Integer);
begin
  { Store the settings so we can restore them next time }
  SetPreviousData(PreviousDataKey, 'WebSitePage.WebServerName', WebSitePage.Values[0]);
  SetPreviousData(PreviousDataKey, 'WebSitePage.WebSiteName', WebSitePage.Values[1]);
  SetPreviousBool(PreviousDataKey, 'EmailOptionsPage.UseSMTP', EmailOptionsPage.Values[0]);
  SetPreviousBool(PreviousDataKey, 'EmailOptionsPage.UseSSL', EmailOptionsPage.Values[1]);
  SetPreviousData(PreviousDataKey, 'EmailPage.SMTPServer', EmailPage.Values[0]);
  SetPreviousData(PreviousDataKey, 'EmailPage.UserId', EmailPage.Values[1]);
  SetPreviousData(PreviousDataKey, 'EmailPage.Port', EmailPage.Values[3]);
  SetPreviousData(PreviousDataKey, 'WebSitePage.WebServerName', WebSitePage.Values[0]);
  SetPreviousData(PreviousDataKey, 'WebSitePage.WebSiteName', WebSitePage.Values[1]);
  SetPreviousData(PreviousDataKey, 'DatabasePage.DatabaseServerName', DatabasePage.Values[0]);
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
	ResultCode: Integer;
begin
	if CurStep = ssPostInstall then begin
		if (IsComponentSelected('website')) then
		begin
			if Exec(ExpandConstant('{app}\website\CodeReviewer.deploy.cmd'), 
              '/T /Y',
              ExpandConstant('{app}\website'), 
              SW_SHOW,	
              ewWaitUntilTerminated, 
              ResultCode) = False then
			begin
				MsgBox('Failed to deploy website using error: ' + IntToStr(ResultCode) + ' ' + ExpandConstant('"{app}\website\CodeReviewer.deploy.cmd" /T'), mbError, MB_OK);
			end
      else begin
          {update web.config}
      end;
		end;
	end;
end;
