@echo off
setlocal

set "AppName=shrink"
set "ProjectFile=%~dp0shrink.csproj"
set "Configuration=Release"

pushd "%~dp0"
if errorlevel 1 exit /B 1

rem .NET Framework uses the installed Windows runtime; do not bundle a .NET runtime.
dotnet publish "%ProjectFile%" -c %Configuration%
if errorlevel 1 goto :failed

rem Read the authoritative output location instead of assuming a source-local folder.
set "PublishDir="
for /f "delims=" %%P in ('dotnet msbuild "%ProjectFile%" -getProperty:PublishDir -p:Configuration^=%Configuration%') do set "PublishDir=%%P"
if not defined PublishDir (
  echo Unable to determine the evaluated PublishDir.
  goto :failed
)
if not exist "%PublishDir%%AppName%.exe" (
  echo Published executable not found in "%PublishDir%".
  goto :failed
)

echo Published to "%PublishDir%"
if /I "%~1"=="--install" (
  if not exist "%UserProfile%\Bin" mkdir "%UserProfile%\Bin"
  if errorlevel 1 goto :failed
  copy /Y "%PublishDir%%AppName%.exe" "%UserProfile%\Bin\"
  if errorlevel 1 goto :failed
  copy /Y "%PublishDir%%AppName%.exe.config" "%UserProfile%\Bin\"
  if errorlevel 1 goto :failed
)

popd
endlocal
exit /B 0

:failed
popd
endlocal
exit /B 1
