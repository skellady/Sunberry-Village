dotnet msbuild SunberryVillage.csproj /t:Restore;Build

@ECHO OFF

if %ERRORLEVEL% NEQ 0 PAUSE