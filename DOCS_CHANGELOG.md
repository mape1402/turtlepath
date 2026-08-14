# TurtlePath Documentation Changelog

All notable changes to TurtlePath versioned documentation will be documented in this file.

Documentation has its own release marker in `.docs.release` and its own GitHub Actions workflow. A documentation version can support multiple `TurtlePath.Template` versions through `docs/guides/template/guide-manifest.json`.

## [docs-v1.2.1] - 2026-08-14

### Added

- Added the versioned `TurtlePath.Template.Documentation` package.
- Added the cumulative template-to-guide version map used by TurtlePath Studio.
- Added the package release workflow for NuGet trusted publishing.

## [docs-v1.2.0] - 2026-08-12

### Added

- Added the central versioned source for the TurtlePath Template use guide.
- Added automatic guide manifest generation from `.docs.release` and `.template.release`.
- Added validation so the English and Spanish guides exist before the manifest is accepted.
- Added guidance for customizing CId with entity-specific profiles when legacy entities use a non-standard identifier backing type.
