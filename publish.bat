@echo off
powershell.exe -NoProfile -File "%~dp0scripts\publish.ps1" %*
exit /b %ERRORLEVEL%
