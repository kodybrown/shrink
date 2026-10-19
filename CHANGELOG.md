# Changelog

## Unreleased

### 2026-10-19 Dotask workflows and Roaming settings installer

- Configure dotask with the solution/project, Release defaults, shared-task tracking,
  and an explicit self-extracting installer recipe. Add Framework-aware publish,
  smoke-test/verification, installer creation, and separate installation tasks.
  Replace the direct-copy publish batch script with the project tasks.
- Discover settings from an explicit existing `--config-file`, executable-stem
  files in the working/executable directories, then `%APPDATA%\shrink\shrink.settings`.
  Show the selected file, reject malformed selected values without fallback, and
  create missing directories only on explicit writes. Keep .NET Framework 4.0
  Client Profile and the project file as the single version source.
- Package a Windows x64 ZIP self-extracting launcher with native doinstall tools.
  Initialize commented plain-text defaults in Roaming AppData without overwriting
  existing settings or placing a file beside the versioned application executable.
  Include the README and standalone MIT license; ignore generated deliveries.
- Verification: `dotask verify`, Framework publishing, evaluated external MSBuild
  output paths, and temporary installer acceptance passed, including a distinct
  version upgrade, settings/ownership preservation, command invocation, uninstall,
  and configured `dotask install` validation. Persistent user/machine PATH and
  active installations were unchanged.

## 1.5.0 - 2026-10-18

- Preserve .NET Framework 4.0 Client Profile while converting to an SDK-style
  project. Replace machine-specific GitVersion/MSBuild Community Tasks with one
  explicit release version and SDK-generated metadata outside the source tree.
- Add `--version` / `-v`, expand CLI help, and document all options, examples,
  saved settings, host limitations, build/publish steps, and exit codes.
- Fix skipped dimensions so they bypass saved defaults, including two successive
  skips for centering without changing dimensions.
- Report caught resize failures with exit code 3. Reject resizing without a usable
  console before writing settings; allow redirected help/configuration display.
- Quote settings paths passed to Notepad. Repair `publish.bat` to publish the root
  project using evaluated `PublishDir`, with installation only via `--install`.
- Verify Release and Debug builds, publishing, version overrides, and isolated
  Windows console smoke tests covering resize/default/skip/config/query/error
  behavior. Confirm build, intermediate, and publish paths remain external.
