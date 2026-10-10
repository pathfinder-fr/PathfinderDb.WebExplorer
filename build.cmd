@echo off
setlocal
dotnet build "%~dp0PathfinderDb.sln" --configuration Release
exit /b %ERRORLEVEL%
