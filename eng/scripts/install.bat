@echo off
setlocal

set "REPO_ROOT=%~dp0..\.."
set "PROJECT=%REPO_ROOT%\src\Spec.Cli\Spec.Cli.csproj"
set "NUPKG_DIR=%REPO_ROOT%\src\Spec.Cli\nupkg"
set "PACKAGE_ID=QuinntyneBrown.Spec.Cli"

echo Packing Spec CLI...
dotnet pack "%PROJECT%" -o "%NUPKG_DIR%" --configuration Release
if %errorlevel% neq 0 (
    echo ERROR: dotnet pack failed.
    exit /b 1
)

echo Uninstalling any existing version of Spec CLI...
dotnet tool uninstall --global %PACKAGE_ID% 2>nul

echo Installing Spec CLI as a global tool...
dotnet tool install --global --add-source "%NUPKG_DIR%" %PACKAGE_ID%

if %errorlevel% neq 0 (
    echo ERROR: Installation failed.
    exit /b 1
)

echo Spec CLI installed successfully. Run 'spec-cli --help' to get started.
endlocal
