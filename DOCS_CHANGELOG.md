# TurtlePath Documentation Changelog

All notable changes to TurtlePath versioned documentation will be documented in this file.

Documentation has its own release marker in `.docs.release` and its own GitHub Actions workflow. A documentation version can support multiple `TurtlePath.Template` versions through `docs/guides/template/guide-manifest.json`.

## [docs-v1.2.4] - 2026-08-19

### Fixed

- Promoted the cumulative template-to-guide manifest so Studio can resolve the current template version and retain access to earlier guide mappings.

## [docs-v1.2.3] - 2026-08-19

### Changed

- Updated the template-to-guide map so template version `1.6.5` uses guide version `1.2.2`.

## [docs-v1.2.2] - 2026-08-14

### Added

- Added Pigeon 2.4.0 guidance for consumer concurrency, queue capacity, handler timeouts, and outbox batch-size tuning.
- Added the English and Spanish guide package version `1.2.2`.

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
