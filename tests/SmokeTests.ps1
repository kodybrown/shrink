param(
    [string] $ExecutablePath,
    [switch] $ConsoleWorker,
    [string] $ResultPath
)

$ErrorActionPreference = 'Stop'

if ($ConsoleWorker) {
    # A separate hidden Console Host gives native children real console handles.
    # All settings changes target a disposable executable copy, never the user's.
    try {
        function Assert-Equal($Actual, $Expected, [string] $Name) {
            if ($Actual -ne $Expected) { throw "$Name : expected '$Expected', got '$Actual'." }
        }
        function Invoke-Shrink([string] $Arguments, [int] $Expected = 0) {
            $info = New-Object System.Diagnostics.ProcessStartInfo
            $info.FileName = $ExecutablePath
            $info.Arguments = $Arguments
            $info.UseShellExecute = $false
            $process = [System.Diagnostics.Process]::Start($info)
            $process.WaitForExit()
            Assert-Equal $process.ExitCode $Expected "Exit code for '$Arguments'"
            $process.Dispose()
        }

        if ([Console]::LargestWindowWidth -lt 110 -or [Console]::LargestWindowHeight -lt 36) {
            throw 'Smoke tests require a Console Host that supports at least 110 columns and 36 rows.'
        }
        [Console]::BufferWidth = [Math]::Max(110, [Console]::BufferWidth)
        [Console]::BufferHeight = [Math]::Max(40, [Console]::BufferHeight)
        [Console]::SetWindowSize(90, 28)
        $settings = "$ExecutablePath.settings"

        Invoke-Shrink ''
        Assert-Equal ([Console]::WindowWidth) 90 'No-config width'
        Assert-Equal ([Console]::WindowHeight) 28 'No-config height'
        Assert-Equal (Test-Path -LiteralPath $settings) $false 'No implicit settings creation'

        Invoke-Shrink '100 30 --config'
        Assert-Equal ([Console]::WindowWidth) 100 'Explicit width'
        Assert-Equal ([Console]::WindowHeight) 30 'Explicit height'
        $saved = [IO.File]::ReadAllText($settings)
        if ($saved -notmatch 'width=100' -or $saved -notmatch 'height=30') { throw 'Dimensions were not saved.' }

        [Console]::SetWindowSize(90, 28)
        Invoke-Shrink '- 32'
        Assert-Equal ([Console]::WindowWidth) 90 'Skipped width bypasses saved width'
        Assert-Equal ([Console]::WindowHeight) 32 'Height-only resize'
        Invoke-Shrink '95 -'
        Assert-Equal ([Console]::WindowWidth) 95 'Width-only resize'
        Assert-Equal ([Console]::WindowHeight) 32 'Skipped height bypasses saved height'

        Invoke-Shrink '- - center'
        Assert-Equal ([Console]::WindowWidth) 95 'Center-only width'
        Assert-Equal ([Console]::WindowHeight) 32 'Center-only height'
        Invoke-Shrink '--querywidth' 95
        Invoke-Shrink '--queryheight' 32
        Invoke-Shrink '--config'
        Assert-Equal ([IO.File]::ReadAllText($settings)) $saved 'Read-only config preserves settings'

        Invoke-Shrink ''
        Assert-Equal ([Console]::WindowWidth) 100 'Saved width reapplied'
        Assert-Equal ([Console]::WindowHeight) 30 'Saved height reapplied'
        Invoke-Shrink '- - !center --config'
        if ([IO.File]::ReadAllText($settings) -notmatch 'center=false') { throw 'Center preference was not saved.' }
        $saved = [IO.File]::ReadAllText($settings)
        Invoke-Shrink '90 28 --config --!config'
        Assert-Equal ([IO.File]::ReadAllText($settings)) $saved 'Config cancellation preserves settings'
        Invoke-Shrink '--does-not-exist' 1
        Invoke-Shrink '90 28 1' 2
        Invoke-Shrink '--clear --config'
        Assert-Equal ([IO.File]::ReadAllText($settings)) '' 'Clear settings'
        Assert-Equal ([Console]::WindowWidth) 90 'Clear/config does not resize width'
        Assert-Equal ([Console]::WindowHeight) 28 'Clear/config does not resize height'

        [IO.File]::WriteAllText($ResultPath, 'PASS: console resizing, defaults, skips, centering, queries, config persistence/cancellation/clear, and argument errors.')
        exit 0
    } catch {
        [IO.File]::WriteAllText($ResultPath, "FAIL: $_")
        exit 1
    }
}

$project = Join-Path $PSScriptRoot '..\shrink.csproj'
if (-not $ExecutablePath) {
    $ExecutablePath = & dotnet msbuild $project -p:Configuration=Release -getProperty:TargetPath
    if ($LASTEXITCODE -ne 0) { throw 'Could not evaluate TargetPath.' }
}
$ExecutablePath = (Resolve-Path -LiteralPath $ExecutablePath).Path
$tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$sandbox = Join-Path $tempRoot ("shrink-smoke-" + [Guid]::NewGuid().ToString('N') + ' space')
New-Item -ItemType Directory -Path $sandbox | Out-Null
try {
    $copy = Join-Path $sandbox 'shrink.exe'
    Copy-Item -LiteralPath $ExecutablePath -Destination $copy
    Copy-Item -LiteralPath "$ExecutablePath.config" -Destination "$copy.config"
    $fileVersion = [Diagnostics.FileVersionInfo]::GetVersionInfo($copy)
    foreach ($option in @('--version', '-v', '/VERSION')) {
        $output = & $copy $option
        if ($LASTEXITCODE -ne 0 -or $output -ne "shrink $($fileVersion.ProductVersion)") {
            throw "Version mismatch for $option : $output"
        }
    }
    $help = & $copy --help | Out-String
    if ($LASTEXITCODE -ne 0 -or $help -notmatch '--version' -or $help -notmatch '--querywidth') {
        throw 'Help failed with redirected output.'
    }
    if (Test-Path -LiteralPath "$copy.settings") { throw 'Informational commands created settings.' }

    # Explicit redirection exercises the no-console failure without changing defaults.
    $info = New-Object Diagnostics.ProcessStartInfo
    $info.FileName = $copy
    $info.Arguments = '80 25 --config'
    $info.UseShellExecute = $false
    $info.RedirectStandardOutput = $true
    $info.RedirectStandardError = $true
    $process = [Diagnostics.Process]::Start($info)
    $process.WaitForExit()
    if ($process.ExitCode -ne 3) { throw "Expected no-console exit code 3, got $($process.ExitCode)." }
    $process.Dispose()
    if (Test-Path -LiteralPath "$copy.settings") { throw 'No-console failure wrote settings.' }

    $result = Join-Path $sandbox 'result.txt'
    $worker = Start-Process "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe" -WindowStyle Hidden -PassThru -Wait -ArgumentList @(
        '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', "`"$PSCommandPath`"",
        '-ConsoleWorker', '-ExecutablePath', "`"$copy`"", '-ResultPath', "`"$result`""
    )
    if (-not (Test-Path -LiteralPath $result)) { throw 'Console worker did not produce a result.' }
    Get-Content -LiteralPath $result
    if ($worker.ExitCode -ne 0) { throw 'Console smoke tests failed.' }
    Write-Output 'PASS: version metadata/aliases, redirected help, no-console error, and settings isolation.'
} finally {
    # Validate the absolute generated target before recursively removing test data.
    $resolved = (Resolve-Path -LiteralPath $sandbox).Path
    if (-not $resolved.StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase) -or
        (Split-Path $resolved -Leaf) -notlike 'shrink-smoke-* space') {
        throw "Unexpected cleanup path: $resolved"
    }
    Remove-Item -LiteralPath $resolved -Recurse
}
