@setlocal
@echo off

set "AppName=shrink"
set "ProjectFile=.\src\%AppName%\%AppName%.csproj"
set "Configuration=Release"
set "RuntimeIdentifier=win-x64"

pushd "%~dp0"

:: TODO: update the project version in the .csproj file

set "PublishDir="
for /f "delims=" %%P in ('dotnet msbuild "%ProjectFile%" -getProperty:PublishDir -p:Configuration^=%Configuration% -p:RuntimeIdentifier^=%RuntimeIdentifier%') do set "PublishDir=%%P"
if not defined PublishDir (
  echo Unable to determine the evaluated PublishDir.
  popd
  pause
  exit /B 1
)

echo Publishing to "%PublishDir%"
dotnet publish "%ProjectFile%" -c %Configuration% -r %RuntimeIdentifier% --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=false
if errorlevel 1 (
  popd
  pause
  exit /B 1
)

if exist "%UserProfile%\Bin" (
  copy /Y "%PublishDir%%AppName%.exe" "%UserProfile%\Bin\"
  if errorlevel 1 (
    popd
    pause
    exit /B 1
  )
)

popd
endlocal
