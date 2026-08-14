# TurtlePath Studio Changelog

## 1.0.7

### Fixed

- Disabled source and symbols packaging for the payload-only Studio NuGet package so releases publish only a valid `.nupkg`.

## 1.0.6

### Added

- Added versioned documentation package resolution through NuGet.
- Added local caching for documentation packages and rendered guides.

### Changed

- Studio now resolves a template version to its guide through the cumulative documentation manifest.
- Guide loading no longer depends on GitHub, Google Drive, or embedded documentation URLs.

## 1.0.5

### Fixed

- Prepared the Studio release metadata for the current application build.
- Kept the displayed Studio version driven by the semantic release tag (`v*.*.*`).

## 1.0.4

### Fixed

- Changed the default Studio update manifest URL to a stable `studio-latest` release instead of GitHub's repository-wide `latest` release.
- Switched Studio update downloads to PowerShell `Invoke-WebRequest` so Google Drive direct-download URLs behave like the same manual validation command users run locally.
- Improved update manifest validation messages when a configured URL returns HTML or another non-JSON response.
- Updated the Studio release workflow to publish and refresh the stable `studio-latest` release assets.

## 1.0.3

### Added

- Added configurable Studio self-updates through a remote manifest URL.
- Added support for GitHub Releases as the default public update source.
- Added `studio.manifest.json` generation during Studio releases with version, package URL, SHA256, size, and release notes URL.
- Added a dedicated updater helper that replaces Studio files after the running app exits.
- Added Environment settings for update manifest URL, update channel, startup checks, update checks, and update installation.

### Changed

- Studio release workflow now injects the semantic app version from `.studio.release`.
- Sidebar version display is normalized to `v*.*.*`.

## 1.0.2

### Added

- Added desktop shortcut creation through the Studio .NET tool.
- Added `turtlepath-studio update` support in the Studio .NET tool.

## 1.0.1

### Changed

- Centered the Studio window on startup.

## 1.0.0

### Added

- Added the first TurtlePath Studio Windows release.
- Added project creation flows for API/Consumer and one-shot Job template hosts.
- Added environment validation and template installation flows.
