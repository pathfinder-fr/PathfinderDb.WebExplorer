@echo off
setlocal

set "currentBranch="
for /f "delims=" %%B in ('git -C "%~dp0." rev-parse --abbrev-ref HEAD 2^>nul') do set "currentBranch=%%B"
if not "%currentBranch%"=="master" exit /b 1

call git -C "%~dp0." reset --hard
if errorlevel 1 exit /b %ERRORLEVEL%

call git -C "%~dp0." pull --ff-only origin master
if errorlevel 1 exit /b %ERRORLEVEL%

call "%~dp0publish.bat"
exit /b %ERRORLEVEL%
