@echo off
setlocal
cd /d "%~dp0"
set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
"%CSC%" /nologo /t:winexe /codepage:65001 /utf8output /win32icon:"%~dp0src\app.ico" /out:"%~dp0SSH-Key-Manager.exe" /r:System.Windows.Forms.dll /r:System.Drawing.dll "%~dp0src\Program.cs"
if errorlevel 1 exit /b 1
echo Built: %~dp0SSH-Key-Manager.exe
