; LANDIS-II Extension infomation
#define CoreRelease "LANDIS-II-V7"
#define ExtensionName "Base Hurricane"
#define AppVersion "1.1"
#define AppPublisher "LANDIS-II Foundation"
#define AppURL "http://www.landis-ii.org/"

; Build directory
#define BuildDir "..\..\src\bin\Debug"

; LANDIS-II installation directories
#define ExtDir "C:\Program Files\LANDIS-II-v7\extensions"
#define AppDir "C:\Program Files\LANDIS-II-v7"
#define LandisPlugInDir "C:\Program Files\LANDIS-II-v7\plug-ins-installer-files"
#define ExtensionsCmd AppDir + "\commands\landis-ii-extensions.cmd"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{74A7EF72-64A5-4846-ACAB-17F686F66D76}
AppName={#CoreRelease} {#ExtensionName}
AppVersion={#AppVersion}
; Name in "Programs and Features"
AppVerName={#CoreRelease} {#ExtensionName} v{#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
DefaultDirName={commonpf}\{#ExtensionName}
DisableDirPage=yes
DefaultGroupName={#ExtensionName}
DisableProgramGroupPage=yes
LicenseFile=LANDIS-II_Binary_license.rtf
OutputDir={#SourcePath}
OutputBaseFilename={#CoreRelease} {#ExtensionName} {#AppVersion}-setup
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"


[Files]
; This .dll IS the extension (ie, the extension's assembly)
; NB: Do not put an additional version number in the file name of this .dll
; (The name of this .dll is defined in the extension's \src\*.csproj file)
Source: {#BuildDir}\Landis.Extension.BaseHurricane-v1.dll; DestDir: {#ExtDir}; Flags: ignoreversion

; Requisite auxiliary libraries
; NB. These libraries are used by other extensions and thus are never uninstalled.
Source: {#BuildDir}\Landis.Library.Metadata-v2.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall ignoreversion
Source: {#BuildDir}\Landis.Library.Cohorts-v2.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall ignoreversion
Source: {#BuildDir}\Landis.Library.AgeOnlyCohorts-v3.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall ignoreversion


; Complete example for testing the extension
Source: ..\examples\*.txt; DestDir: {#AppDir}\examples\{#ExtensionName}; Flags: ignoreversion
Source: ..\examples\*.gis; DestDir: {#AppDir}\examples\{#ExtensionName}; Flags: ignoreversion
Source: ..\examples\*.bat; DestDir: {#AppDir}\examples\{#ExtensionName}; Flags: ignoreversion


; LANDIS-II identifies the extension with the info in this .txt file
; NB. New releases must modify the name of this file and the info in it
#define InfoTxt "Base Hurricane 1.0.txt"
Source: {#InfoTxt}; DestDir: {#LandisPlugInDir}
; NOTE: Don't use "Flags: ignoreversion" on any shared system files


[Run]
Filename: {#ExtensionsCmd}; Parameters: "remove ""Base Hurricane"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#ExtensionsCmd}; Parameters: "add ""{#InfoTxt}"" "; WorkingDir: {#LandisPlugInDir} 


[UninstallRun]
; Remove "Age-Only Succession" from "extensions.xml" file.
Filename: {#ExtensionsCmd}; Parameters: "remove ""Base Hurricane"" "; WorkingDir: {#LandisPlugInDir}


