# Changelog

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
