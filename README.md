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
| `--config-file <path>` | Use exactly this existing settings file; no discovery or fallback. |
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

Settings use the existing `name=value` format:

```ini
width=100
height=30
center=true
```

Discovery follows this order:

1. `--config-file <path>` selects exactly that existing file. Relative paths start
   at the working directory. A missing, unreadable, or invalid file is an error;
   an explicit selection never falls back.
2. `<executable-stem>.settings` in the working directory.
3. The same filename beside the executable.
4. `%APPDATA%\shrink\shrink.settings` (**Roaming**, not Local AppData).

For example, `shrink.exe` searches for `shrink.settings`; `custom.tool.exe`
searches locally for `custom.tool.settings`. The Roaming fallback always remains
`shrink\shrink.settings`. The former `shrink.exe.settings` name is not searched
or migrated automatically.

A local file takes precedence over Roaming settings. `--config` displays the full
selected path; saving, clearing, and `--edit` all use that same file. When ordinary
discovery finds no file, shrink uses its normal current-window defaults. Reads,
help, and configuration display never create files or directories. An explicit
save/clear creates the Roaming directory when needed. The selected directory must
be writable when saving or clearing values.

```powershell
# Inspect normal discovery, including the selected path.
shrink --config

# Select an existing portable configuration; option position does not matter.
shrink 100 30 --config --config-file 'C:\Tools\my shrink.settings'
```

Only explicitly supplied dimensions and centering values are saved by `--config`;
skipped dimensions are not saved. Existing keys remain unless cleared. `#` and `;`
start comment lines. Saving rewrites the file and does not preserve comments.
Dimensions must be positive integers; `center` must be `true` or `false`.

## Build and publish

Build on Windows with a current .NET SDK (the shared C# tasks require .NET 10),
MSBuild 17.8 or newer, and dotask. The .NET Framework 4.0 reference assemblies must
be installed or available through SDK restore from NuGet. End users only need
.NET Framework, not dotask or a .NET SDK.

```powershell
cd C:\path\to\shrink
dotask build
dotask publish
```

`.dotasks.yaml` configures `shrink.sln`, `shrink.csproj`, and Release build defaults.
Project `publish`, `test`, and `verify` tasks supplement the installed shared .NET
tasks. Publishing uses `dotnet publish` without modern runtime bundling or a runtime
identifier. Build, intermediate, and publish locations follow the evaluated
user-level MSBuild output policy. Publishing does not install the application.

To run the published executable:

```powershell
$publishDir = dotnet msbuild shrink.csproj -p:Configuration=Release -getProperty:PublishDir
& (Join-Path $publishDir 'shrink.exe') --version
& (Join-Path $publishDir 'shrink.exe') --help
```

## Self-extracting installer

```powershell
dotask create-installer
# Separate explicit action: launch an existing installer.
dotask install
```

Creation publishes shrink, reads its version from MSBuild, stages the exact payload,
and uses doinstall's shared build/gather and self-extracting assembly tasks.
Rust/Cargo 1.95+ is needed for the build/gather task. No Rust SDK, dotask, ZIP tool,
or modern .NET runtime is needed by someone running the installer.

The initial installer targets Windows x64 and is written beneath
`artifacts/installers/windows-x64/`. Its reported runnable artifact is
`shrink-setup-v<version>-se.exe`. `--app-version` overrides both the application
build and package version together. Existing deliverables are never overwritten.

The recipe uses **prebuilt** tools from the evaluated Cargo output of
`settings.doinstall.source` (`../doinstall`). It never builds, installs, or updates
doinstall implicitly. These tools must support `${app-data}` and plain-text
`settings-files`. To select another matching prebuilt tool set:

```powershell
dotask create-installer --doinstall-tools C:\Tools\doinstall
```

The installer defaults to `%LOCALAPPDATA%\Programs\shrink` for application files,
creates a stable `shrink` command, and offers adding its command directory to the
user PATH. Its separate editable settings file is initialized at
**`%APPDATA%\shrink\shrink.settings`**, outside the versioned application builds.
The initial file contains commented examples, so it does not force an 80 x 25
window. Existing settings are preserved byte-for-byte on installation and upgrade.
No settings file is placed beside the executable to shadow the Roaming file.

Uninstall preserves settings by default. The retained installer supports
`uninstall --remove-settings` for settings it originally created; preexisting
unowned settings are never adopted or removed. The self-extracting launcher retains
its extracted installer sources in a fresh temporary directory; `--extract-to`
selects a new or empty extraction directory.

## Verification

```powershell
dotask verify
# The standalone smoke suite also accepts -ExecutablePath <built-or-published-exe>.
.\tests\SmokeTests.ps1
# Validate installation/upgrade/uninstall in temporary roots without changing PATH.
.\tests\InstallerTests.ps1 -Installer <reported-installer-path>
# Add -UpgradeInstaller <other-version-installer> to check a distinct build upgrade.
```

Project verification checks prerequisites, builds, runs settings-discovery and
hidden-console smoke tests, then checks Git whitespace. All application tests use
disposable executable/configuration copies and child-process Roaming overrides;
they never read or modify your installed settings. Installer acceptance checks
extraction, validation, command invocation, upgrade preservation, and uninstall.

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
| `4` | Settings selection, validation, path, or file-access error. |

`--querywidth` and `--queryheight` return the requested dimension instead of these
normal success codes. They require an attached console. Other unhandled errors can produce a runtime failure code. Some hosts silently ignore
window operations; a zero exit code cannot prove that an unsupported host obeyed
them.

## License

MIT. See [LICENSE.txt](LICENSE.txt); the source-file headers also include the license.
