@echo off
setlocal
cd /d "%~dp0"
set "PUBLISH_EXE=%~dp0bin\Release\net8.0-windows\win-x64\publish\RMSThemeStudio.exe"
if exist "%PUBLISH_EXE%" (
    start "" "%PUBLISH_EXE%"
) else (
    echo [RMS Theme Studio] Building and launching from source...
    dotnet run --project "%~dp0RMSThemeStudio.csproj" -c Release
)
endlocal
