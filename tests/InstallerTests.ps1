param(
    [Parameter(Mandatory = $true)][string] $Installer,
    [string] $UpgradeInstaller
)
$ErrorActionPreference = 'Stop'
$Installer = (Resolve-Path -LiteralPath $Installer).Path
if ($UpgradeInstaller) { $UpgradeInstaller = (Resolve-Path -LiteralPath $UpgradeInstaller).Path }
$tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$sandbox = Join-Path $tempRoot ('shrink-installer-test-' + [Guid]::NewGuid().ToString('N') + ' space')
New-Item -ItemType Directory -Path $sandbox | Out-Null
$roaming = Join-Path $sandbox 'redirected roaming'
$local = Join-Path $sandbox 'local'
$root = Join-Path $sandbox 'application'
$settings = Join-Path $roaming 'shrink\shrink.settings'
$userPath = [Environment]::GetEnvironmentVariable('Path', 'User')
$machinePath = [Environment]::GetEnvironmentVariable('Path', 'Machine')
$success = $false
function Invoke-Native([string] $Program, [string[]] $Arguments) {
    $info = New-Object Diagnostics.ProcessStartInfo
    $info.FileName = $Program
    $info.Arguments = ($Arguments | ForEach-Object { '"' + $_ + '"' }) -join ' '
    $info.WorkingDirectory = $sandbox
    $info.UseShellExecute = $false
    # Override only child environment; never alter the user's profile or PATH.
    $info.EnvironmentVariables['APPDATA'] = $roaming
    $info.EnvironmentVariables['LOCALAPPDATA'] = $local
    $info.RedirectStandardOutput = $true
    $info.RedirectStandardError = $true
    $process = [Diagnostics.Process]::Start($info)
    $output = $process.StandardOutput.ReadToEnd()
    $errorText = $process.StandardError.ReadToEnd()
    $process.WaitForExit()
    $exitCode = $process.ExitCode
    $process.Dispose()
    if ($exitCode -ne 0) { throw "Exit $exitCode from $Program : $errorText $output" }
    return $output
}
function Install-Package([string] $Package, [string] $Extract, [switch] $Validate) {
    $arguments = @('--extract-to', $Extract, '--non-interactive', '--install-dir', $root, '--set', 'add-to-path=false', '--set', 'additional-command=false')
    if ($Validate) { $arguments += '--validate' }
    Invoke-Native $Package $arguments | Out-Null
}
try {
    Install-Package $Installer (Join-Path $sandbox 'validation') -Validate
    if ((Test-Path -LiteralPath $root) -or (Test-Path -LiteralPath $roaming)) { throw 'Validation created application/settings state.' }
    Install-Package $Installer (Join-Path $sandbox 'initial extraction')
    if (-not (Test-Path -LiteralPath $settings)) { throw 'Installer did not create Roaming shrink.settings.' }
    $original = [IO.File]::ReadAllText($settings)
    if ($original -notmatch '# width=80') { throw 'Default settings examples were not copied.' }
    $command = Join-Path $root 'shrink.exe'
    if ((Invoke-Native $command @('--version')) -notmatch '^shrink ') { throw 'Installed command did not launch shrink.' }
    $configuration = Invoke-Native $command @('--config')
    if (-not $configuration.Contains($settings)) { throw 'Installed command did not select the Roaming settings.' }
    if (Get-ChildItem -LiteralPath (Join-Path $root 'app') -Recurse -File | Where-Object { $_.Name -eq 'shrink.settings' -and $_.Directory.Name -ne 'defaults' }) {
        throw 'An executable-adjacent settings file could shadow Roaming settings.'
    }
    $edited = "# customized by the user`r`nwidth=117`r`nheight=31`r`ncenter=false`r`n"
    [IO.File]::WriteAllText($settings, $edited)
    $next = if ($UpgradeInstaller) { $UpgradeInstaller } else { $Installer }
    Install-Package $next (Join-Path $sandbox 'upgrade extraction')
    if ([IO.File]::ReadAllText($settings) -ne $edited) { throw 'Upgrade overwrote settings.' }
    if ((Invoke-Native $command @('--config')) -notmatch 'width  = 117') { throw 'Upgraded command did not use saved settings.' }
    if ($UpgradeInstaller) {
        $builds = @(Get-ChildItem -LiteralPath (Join-Path $root 'app') -Directory)
        if ($builds.Count -lt 2) { throw 'Upgrade did not retain two distinct application builds.' }
    }
    $uninstaller = Join-Path $root 'installer\installer.exe'
    Invoke-Native $uninstaller @('uninstall', '--non-interactive', '--leave-settings') | Out-Null
    if ((Test-Path -LiteralPath $command) -or [IO.File]::ReadAllText($settings) -ne $edited) { throw 'Uninstall did not remove the command and preserve settings.' }

    # A later fresh installation cannot adopt and remove the user's existing file.
    Install-Package $Installer (Join-Path $sandbox 'preexisting extraction')
    Invoke-Native $uninstaller @('uninstall', '--non-interactive', '--remove-settings') | Out-Null
    if ([IO.File]::ReadAllText($settings) -ne $edited) { throw 'Preexisting settings were adopted or removed.' }
    $success = $true
    Write-Output 'PASS: self-extraction, read-only validation, Roaming initialization, stable command, settings preservation on upgrade/reinstall, uninstall, and ownership.'
} finally {
    if ([Environment]::GetEnvironmentVariable('Path', 'User') -ne $userPath -or [Environment]::GetEnvironmentVariable('Path', 'Machine') -ne $machinePath) {
        throw 'Installer acceptance changed persistent PATH.'
    }
    if ($success) {
        $resolved = (Resolve-Path -LiteralPath $sandbox).Path
        if (-not $resolved.StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase) -or (Split-Path $resolved -Leaf) -notlike 'shrink-installer-test-* space') {
            throw "Unexpected cleanup target: $resolved"
        }
        Remove-Item -LiteralPath $resolved -Recurse
    } else {
        Write-Output "Failed acceptance fixture retained: $sandbox"
    }
}
