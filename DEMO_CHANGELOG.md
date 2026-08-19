# TurtlePath Demo Templates Changelog

## [demo-v1.4.8] - 2026-08-19

### Fixed

- Updated the generated Heroes Showcase TurtlePath package default and fallback to `1.6.3`.
- Updated generated transaction boundary registration guidance to use explicit Business and API assemblies.

## [demo-v1.4.7] - 2026-08-19

### Fixed

- Updated the generated Heroes Showcase TurtlePath package fallback from `1.6.1` to the published `1.6.2` release.

## [demo-v1.4.6] - 2026-08-14

### Changed

- Updated the generated Heroes Showcase Pigeon packages to 2.4.0.
- Added a consumer throughput and concurrency example to the generated guide.

## [demo-v1.4.5] - 2026-08-14

### Changed

- Updated the generated Heroes Showcase package references to the published `1.6.1` release.
- Switched the generated transaction boundary integration to the published `TurtlePath.Spider.Transactions` package without local project references.

## [demo-v1.4.4] - 2026-08-13

### Added

- Added the `--turtlepath-version` template parameter so generated Heroes Showcase projects can select the TurtlePath NuGet package version at creation time.
- Centralized generated TurtlePath package references through `TurtlePathVersion` in `Directory.Build.targets`.

## [demo-v1.4.3] - 2026-08-13

### Added

- Added a dedicated release workflow for `TurtlePath.Template.HeroesShowcase`.
- Added release version stamping for the Heroes Showcase template metadata.

