@echo off
echo Building Gamma Manager...
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe "v:\Coding\Gamma-Manager-1.2\Gamma Manager\Gamma Manager.csproj" /p:Configuration=Debug /t:Rebuild /v:n /fl /flp:logfile=v:\Coding\Gamma-Manager-1.2\build_output.txt;verbosity=detailed
echo Build complete. Exit code: %ERRORLEVEL%
if exist "v:\Coding\Gamma-Manager-1.2\build_output.txt" (
    echo.
    echo Last 50 lines of build log:
    powershell -Command "Get-Content 'v:\Coding\Gamma-Manager-1.2\build_output.txt' | Select-Object -Last 50"
)
