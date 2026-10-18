# shrink

A small Windows command-line utility for resizing and optionally centering the
current console window. Dimensions are measured in columns and rows.

```powershell
shrink 80 25
shrink 120 35 center
shrink --help
```

Use a classic Windows Console Host window running Command Prompt or PowerShell.
Window resizing and positioning rely on Windows console APIs. Windows Terminal,
VS Code terminals, and other pseudoconsole hosts may ignore or reject these
operations; they are not supported for window control.

The executable targets **.NET Framework 4.0 Client Profile**. It uses the installed
.NET Framework runtime rather than bundling a modern .NET runtime. Later .NET
Framework 4.x releases can run applications targeting 4.0; see Microsoft's
[version compatibility documentation](https://learn.microsoft.com/en-us/dotnet/framework/migration-guide/version-compatibility).

## Usage

```text
shrink [width|-] [height|-] [center|!center] [options]
```

Width comes before height. Options are case-insensitive and accept `--`, `-`, `/`,
or no prefix: `--center`, `-center`, `/center`, and `center` are equivalent.
A standalone `-` skips the next dimension and keeps its current value, even when
that dimension has a saved default. A third `-` skips centering.

With no arguments, shrink applies saved settings. An unspecified dimension uses
its saved value, or the current value when no default is saved. Centering is off
unless enabled explicitly or in saved settings. **80 x 25 is an example, not a
built-in default.** Requested dimensions are limited to the console's available
screen size. Width is at least 8 columns; numeric dimensions below 4 are raised
to 4 before screen-size limits are applied.

| Argument or option | Effect |
| --- | --- |
| `width` | Set the width in columns. |
| `height` | Set the height in rows. Use `-` first to skip width. |
| `-` | Keep the next dimension at its current size, bypassing its saved default. |
| `center` / `!center` | Enable or disable centering for this invocation. |
| `--help`, `-h`, `/?` | Show usage, examples, and current/saved settings; then exit. |
| `--version`, `-v` | Print the release version; then exit. |
| `--config` | With dimensions or centering: save the explicitly supplied values and apply them. Alone: display current/saved settings and exit. |
| `--!config` | Cancel an earlier `--config` for this invocation. |
| `--clear` | Immediately clear saved settings, then continue processing the command. |
| `--querywidth` | Return the current width as the process exit code; then exit. |
| `--queryheight` | Return the current height as the process exit code; then exit. |
| `--edit` | Open the settings file in Notepad, then continue processing. |
| `--debug` | Wait for a key after applying the window settings. |
| `--email` | Open an email to the author, then continue processing. |
| `--web` | Open the Bricksoft website, then continue processing. |
| `--src`, `--source` | Open this repository, then continue processing. |
| `--license` | Open the MIT license page, then continue processing. |

Options are processed in order. In particular, `--clear` takes effect immediately,
and help, version, and query options exit as soon as they are reached. Put
`--version` by itself when scripting a version check.

## Examples

```powershell
# Set both dimensions.
shrink 80 25

# Change only width, keeping the current height even if a height is saved.
shrink 120 -

# Change only height, keeping the current width even if a width is saved.
shrink - 40

# Keep both dimensions and center the window.
shrink - - center

# Override saved centering for this invocation.
shrink 100 30 '!center'

# Save a preferred size and centering, and apply them now.
shrink 100 30 center --config

# Apply those saved defaults later.
shrink

# Inspect saved settings without resizing.
shrink --config

# Clear all saved values without resizing.
shrink --clear --config

# Read a dimension in PowerShell. The result is an exit code, not printed text.
shrink --querywidth
$width = $LASTEXITCODE

# Inspect the release version.
shrink --version
```

In Command Prompt, use `%ERRORLEVEL%` for query results. If delayed expansion is
enabled, disable it before using options containing `!`.

## Configuration

Settings are stored beside the executable as `shrink.exe.settings`. For example,
`C:\Tools\shrink.exe` uses `C:\Tools\shrink.exe.settings`, regardless of the current
working directory. The directory must be writable when saving or clearing values.

```ini
width=100
height=30
center=true
```

Only explicitly supplied dimensions and centering values are saved by `--config`;
skipped dimensions are not saved. Existing saved keys are kept unless cleared.
Read-only commands do not create or rewrite the settings file. When editing by
hand, use `name=value` without spaces around boolean values. `#` and `;` start
comment lines. Saving rewrites the file and does not preserve comments.

## Build and publish

Build on Windows with a current .NET SDK providing MSBuild 17.8 or newer and the
.NET Framework 4.0 reference assemblies. If they are not installed, SDK restore
can obtain the reference-assembly package from NuGet. The SDK is needed to build;
users of the executable only need .NET Framework.

```powershell
cd C:\path\to\shrink

dotnet build shrink.sln -c Release
.\publish.bat
```

The publish script reads the evaluated MSBuild `PublishDir`. Build, intermediate,
and publish locations follow your user-level `Directory.Build.props` policy.
There is no automatic installation during build or publish.

To publish and copy `shrink.exe` and its runtime configuration to
`%USERPROFILE%\Bin`, use:

```powershell
.\publish.bat --install
```

Add that directory to `PATH` to run `shrink` from any directory. Installing an
updated executable preserves an existing `shrink.exe.settings` file.

To find and run the published executable in PowerShell:

```powershell
$publishDir = dotnet msbuild shrink.csproj -p:Configuration=Release -getProperty:PublishDir
& (Join-Path $publishDir 'shrink.exe') --version
& (Join-Path $publishDir 'shrink.exe') --help
```

## Verification

After building Release, run the Windows console smoke tests:

```powershell
.\tests\SmokeTests.ps1
```

The tests launch a hidden Console Host and use a disposable copy of the executable
(including a path containing spaces). They check resizing, saved defaults, skipped
dimensions, config persistence, query/argument exit codes, version metadata, and
redirected output without touching installed settings.

## Versioning

`<Version>` in `shrink.csproj` is the single release-version source, currently
`1.5.0`. Use `major.minor.patch`: major for incompatible changes, minor for new
features, and patch for fixes. Edit that property when preparing a release.

The SDK generates assembly, file, and informational versions in the intermediate
output directory. No tracked source is rewritten during a build, and the version
does not depend on the date, commit count, or local Git tools. `shrink --version`
prints the informational version. The SDK's default mapping from `Version` is
documented in [Set assembly attributes](https://learn.microsoft.com/dotnet/standard/assembly/set-attributes-project-file).

For a one-off build, override it without editing the project:

```powershell
dotnet build shrink.csproj -c Release -p:Version=1.5.1
```

## Exit codes

| Code | Meaning |
| --- | --- |
| `0` | Success, including help, version, and configuration display. |
| `1` | Unknown option. |
| `2` | Unexpected extra numeric argument. |
| `3` | No usable attached console, or an error caught while resizing/positioning. |

`--querywidth` and `--queryheight` return the requested dimension instead of these
normal success codes. They require an attached console. File-access and other
unhandled errors can produce a runtime failure code. Some hosts silently ignore
window operations; a zero exit code cannot prove that an unsupported host obeyed
them.

## License

MIT. The full license text is included in the source-file headers.
