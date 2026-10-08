# TurtlePath Demo Templates Changelog

## [demo-v1.7.0] - 2026-10-07

### Added

- Added Heroes Showcase references for `TurtlePath.Automations.AspNetCore` and `TurtlePath.Automations.Pigeon`.
- Added an incident report automation example that exposes both an HTTP endpoint and a Pigeon consumer from the automation profile.
- Added a Heroes Showcase automation endpoint example that builds query values with custom endpoint bindings.
- Wired the Heroes API host to map automation-declared endpoints and register automation-declared Pigeon consumers.

### Changed

- Updated the generated Heroes Showcase TurtlePath package fallback and README examples to `1.10.0`.
- Updated Heroes Showcase documentation for automation-declared entry points.

## [demo-v1.6.1] - 2026-10-05

### Changed

- Updated the generated Heroes Showcase TurtlePath package fallback and README examples to `1.9.1`.
- Prepared the Heroes Showcase demo template release aligned with the TurtlePath `1.9.1` testing and coverage update.
- Updated demo release validation to restore generated projects against locally packed TurtlePath runtime packages before NuGet publication.

## [demo-v1.6.0] - 2026-10-05

### Added

- Added Spider.Pipelines.Web to the Heroes Showcase API.
- Added the Development-only Spider architecture UI and runtime trace endpoints to the Heroes Showcase, including concrete automation operations from the demo profiles.
- Added a custom `ComposeFlow` example to `GetHeroOperationsReportQueryHandler` using the `Heroes.Report` flow profile.

### Changed

- Updated generated Spider.Pipelines references to 2.2.1.
- Updated generated Heroes dependency versions, including Pigeon 4.0.2, Krackend.EventSourcing.EntityFrameworkCore 4.0.3, Scalar.AspNetCore 2.17.13, Microsoft.Extensions 10.0.12, EF Core 10.0.12, Microsoft.Data.Sqlite 10.0.12, and current test tooling.
- Updated the generated Heroes Showcase TurtlePath package fallback to `1.9.0`.
- Updated Heroes Showcase documentation for Spider flows, traces, and the custom operations report flow.

## [demo-v1.5.0] - 2026-09-28

### Changed

- Added Heroes Showcase documentation references for TurtlePath.EventSourcing post-append observers.

## [demo-v1.4.10] - 2026-09-24

### Changed

- Updated generated Heroes Showcase Pigeon messaging packages to 4.0.0.
- Updated generated Heroes Showcase documentation to reference Pigeon 4.0.0.

## [demo-v1.4.9] - 2026-09-08

### Fixed

- Renamed the Heroes Showcase incident command handler folder from `Handlers` to `Commands`.

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

