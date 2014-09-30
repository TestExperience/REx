@ECHO OFF

REM The following directory is for .NET 4.0
set DOTNETFX4=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319

echo Installing REx.LauncherService ...
echo ---------------------------------------------------
%DOTNETFX4%\InstallUtil /u REx.LauncherService.exe
echo ---------------------------------------------------
echo Done.