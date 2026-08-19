# TurtlePath Template Changelog

All notable changes to `TurtlePath.Template` will be documented in this file.

`TurtlePath.Template` has its own release marker in `.template.release` and its own GitHub Actions workflow. Runtime libraries continue to use `CHANGELOG.md`; TurtlePath Studio continues to use `STUDIO_CHANGELOG.md`.

## [template-v1.6.6] - 2026-08-19

### Fixed

- Updated the generated TurtlePath package default and fallback to `1.6.3`.
- Updated generated transaction boundary registration to pass Business and API assemblies explicitly to `TurtlePath.Spider.Transactions`.
- Updated generated template documentation so transaction profile discovery no longer describes global loaded-assembly scanning.

## [template-v1.6.5] - 2026-08-19

### Fixed

- Updated the generated TurtlePath package fallback from `1.6.1` to the published `1.6.2` release.

## [template-v1.6.4] - 2026-08-14

### Changed

- Updated generated Pigeon packages to 2.4.0.
- Added guidance for limiting consumer concurrency and queue capacity with Pigeon 2.4.0.

## [template-v1.6.3] - 2026-08-14

### Changed

- Updated the generated TurtlePath package references to the published `1.6.1` release.
- Switched the generated transaction boundary integration to the published `TurtlePath.Spider.Transactions` package without local project references.

## [template-v1.6.2] - 2026-08-13

### Added

- Added the `--turtlepath-version` template parameter so generated services can select the TurtlePath NuGet package version at creation time.
- Centralized generated TurtlePath package references through `TurtlePathVersion` in `Directory.Build.targets`.

## [template-v1.6.1] - 2026-08-12

### Added

- Added `turtlepath.template.json` to generated services so developers can see which `TurtlePath.Template` package version created the project.
- Added release-time stamping for `turtlepath.template.json` from `.template.release`.
- Added this dedicated template changelog and release marker.

## [template-v1.6.0] - 2026-08-12

### Added

- Added support for custom exception handling profiles discovered from the generated Business and API assemblies.
- Added transaction boundary profiles so feature-specific transaction rules can be extended without editing default pipeline registration.
- Added template guide documentation for exception handling profiles and transaction boundary profiles.

## [template-v1.4.3] - 2026-08-11

### Fixed

- Fixed generated template test hosts so EF Core loads entity configurations from the persistence assembly.

## [template-v1.4.1] - 2026-08-11

### Added

- Added Scalar OpenAPI defaults with the Scalar AI assistant disabled by default.
- Added opt-in EventSourcing wiring and documentation.

### Fixed

- Fixed generated template layering so Business depends on Domain and TurtlePath abstractions while API owns the Persistence reference.
- Fixed generated template package versions so every TurtlePath package reference targets `1.4.1`.

## [template-v1.4.0] - 2026-08-10

### Added

- Added `TurtlePath.Template` as the official NuGet-distributed `dotnet new` template package.
- Added API/consumer and one-shot job host generation through the `turtlepath` template short name.
- Added DataScorpio, OctoMap, Crabalidator, TurtlePath testing foundations, TurtlePath jobs, exception handling, and EF Core defaults to generated projects.
- Added Pigeon Azure Service Bus messaging defaults with EF Core outbox enabled by default and explicit ambient transaction suppression for direct broker publishing.
- Added generated-project validation coverage for both API/consumer and job host modes.
