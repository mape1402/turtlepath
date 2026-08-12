# Changelog

All notable changes to TurtlePath will be documented in this file.

## [v1.5.1] - 2026-08-12

### Added

- Added `turtlepath-studio update` to reinstall TurtlePath Studio from the latest published Studio release.
- Added desktop shortcut creation for `TurtlePath Studio` during Studio installation.
- Added Studio tool usage documentation to the README.

## [v1.5.0] - 2026-08-12

### Added

- Added `TurtlePath.Studio.Tool`, a .NET tool that installs TurtlePath Studio from the latest published Studio GitHub release.
- Added a dedicated TurtlePath Studio GitHub release workflow that publishes the Windows Studio app only when `.studio.release` changes.

### Fixed

- Centered the TurtlePath Studio window on startup.

## [v1.4.3] - 2026-08-11

### Added

- Added a Studio Demos section with a Heroes Showcase card that creates projects from `TurtlePath.Template.HeroesShowcase`.

### Fixed

- Fixed Studio template installation to use the current `package@version` syntax and the NuGet.org source explicitly.
- Fixed generated template test hosts so EF Core loads entity configurations from the persistence assembly.

## [v1.4.2] - 2026-08-11

### Added

- Added `TurtlePath.Template.HeroesShowcase`, a NuGet-distributed `dotnet new` demo template for generating a complete Heroes service with automations, custom handlers, jobs, tests, Spider, DataScorpio, OctoMap, Crabalidator, Pigeon, and EventSourcing examples.

## [v1.4.1] - 2026-08-11

### Added

- Added command response options so create, update, and patch handlers can rebuild mutation responses from storage with configured includes.
- Added automation profile support for `ReloadBeforeResponse()` and `Include(...)` on mutation declarations.
- Added `TurtlePathTestHost.CreateFromServices(...)` and `UseApplicationServices(...)` for integration tests that start from the application's real dependency registration.
- Added `TurtlePath.Spider` with Spider extension methods for sending Pelican requests through Spider boundaries while preserving the concrete request type.
- Added Scalar OpenAPI defaults to the generated service template with the Scalar AI assistant disabled by default.
- Added opt-in EventSourcing wiring and documentation to the generated service template.

### Fixed

- Fixed generated template layering so Business depends on Domain and TurtlePath abstractions while API owns the Persistence reference.
- Fixed generated template package versions so every TurtlePath package reference targets `1.4.1`.

## [v1.4.0] - 2026-08-10

### Added

- Added `TurtlePath.Template` as the official NuGet-distributed `dotnet new` template package.
- Added API/consumer and one-shot job host generation through the `turtlepath` template short name.
- Added DataScorpio, OctoMap, Crabalidator, TurtlePath testing foundations, TurtlePath jobs, exception handling, and EF Core defaults to generated projects.
- Added Pigeon Azure Service Bus messaging defaults with EF Core outbox enabled by default and explicit ambient transaction suppression for direct broker publishing.
- Added generated-project validation coverage for both API/consumer and job host modes.

## [v1.3.1] - 2026-08-10

### Fixed

- Rebuilt `TurtlePath.Analyzers` against Roslyn `5.0.0` so projects using the current .NET 10 SDK can load the analyzer without compiler-version warnings.

## [v1.3.0] - 2026-08-07

### Added

- Added `TurtlePath.Testing` with a reusable test host for TurtlePath handler and automation tests.
- Added `TurtlePath.Testing.EntityFrameworkCore` with SQLite-backed integration test helpers.
- Added `TurtlePath.Testing.EventSourcing` with event stream assertion helpers.
- Added delegate-based mapper and validator adapters for tests without a mocking framework.
- Added in-memory TurtlePath storage implementing reader and writer adapters with operation tracing.
- Added testing helpers for TurtlePath exception handling and one-shot jobs.
- Added test coverage for direct manual handlers, Pelican-dispatched handlers, generated automations, seeded read scenarios, update, delete, paged queries, jobs, exceptions, SQLite persistence, and event stream assertions.
- Added `samples/TurtlePath.Samples.Testing` with copyable testing examples.
- Added an internal testing roadmap for the TurtlePath testing package work.

## [v1.2.0] - 2026-08-07

### Added

- Added `TurtlePath.EventSourcing` as a Krackend-backed bridge from TurtlePath command handler `AfterSave` hooks to event store appends.
- Added event sourcing profiles for compact command/entity to event payload mappings.
- Added support for multiple events per command/entity pair, conditional event appends, and configurable Krackend expected-version policies.
- Added sample coverage for Customer create, update, and patch events stored through the event sourcing hook.

### Notes

- TurtlePath runtime and testing packages target `net9.0` and `net10.0`.

## [v1.1.0] - 2026-08-06

### Added

- Added transport-neutral exception handling core with profile-based exception mappings.
- Added ASP.NET Core, consumer, and worker exception handling adapters.
- Added TurtlePath job infrastructure for one-shot job managers and recurring cron-style background services.
- Added sample coverage for exception handling and jobs.

### Changed

- Updated package dependencies across TurtlePath projects, samples, benchmarks, and tests.
- Kept Entity Framework Core on the latest `9.0.x` patch line while moving TurtlePath runtime and testing packages to `net9.0` and `net10.0`.

## [v1.0.0] - 2026-08-05

### Added

- Created the TurtlePath library using the OctoMap repository structure.
- Extracted the reusable Pelican handler foundation from the Elysium template.
- Added base tests, CI, release workflow, README, changelog, samples, and benchmark project placeholders.
- Documented compatibility surface before package splitting.
- Split the extracted surface into focused packages for identifier, domain, application, persistence, EF Core, mapping, validation, and filtering concerns.
- Moved mapping and validation contracts into dedicated abstraction packages, removed generic bucket folders from source projects, and separated application errors from ASP.NET Core HTTP exceptions.
- Consolidated mapping, validation, and persistence contracts into `TurtlePath.Abstractions`.
- Removed the root composition package, standalone identifier package, CId EF value generator package, serialization package, and Swagger package; identifiers now live under `TurtlePath.Domain.Identifier`.
- Renamed the main handler package from `TurtlePath.Application` to `TurtlePath`.
- Added the EF Core base DbContext with configuration discovery and `CId` value conversion.
- Moved base entity EF key conventions into the EF Core base DbContext.
- Added DI registration for configurable EF Core DbContext conventions and removed the redundant `BaseEntityConfiguration` type.
- Split EF Core model conventions out of `BaseDbContext` into independently registered convention services.
- Added a chainable TurtlePath builder for identifier, EF Core, OctoMap, Crabalidator, and Sieve registrations.
- Added focused AutoMapper and FluentValidation adapter packages.
- Moved EF Core `CId` conversion away from static metadata and into registered identifier definitions.
- Added default and per-entity `CId` definitions so clean schemas can share one identifier type while legacy schemas can override individual entities.
- Removed the static `CIdMetadata` configuration surface.
- Removed `CId.New()` so generated identifiers always come from the configured `ICIdFactory`.
- Added `CIdProfile` configuration so per-entity identifier overrides can be grouped outside the DI registration chain.
- Expanded the basic sample into a layered commerce example with Pelican mediator dispatch, command handlers, SQLite-backed EF Core configuration, scalar CId defaults, per-entity profiles, and hooks.
- Added generic handler contracts for custom entity key types through `IEntity<TKey>`, `IBaseRequest<TKey>`, and `IBaseResponse<TKey>` while keeping the short `BaseEntity`/`CId` handler path.
- Split generic entity handlers from `BaseEntity`/`CId` handlers; generic-key handlers now use `Generic...` names while the recommended `BaseEntity`/`CId` handlers keep the concise names.
- Organized generic handler source files under `Generic` folders while keeping the `BaseEntity`/`CId` handlers directly under `Commands` and `Queries`.
- Extracted internal command and query hook stage runners to centralize handler hook execution and protect stage order with tests.
- Replaced static hook runner behavior with dependency-injected hook runner services.
- Restored no-response create, update, patch, and delete command handlers for both generic-key and BaseEntity/CId flows.
- Grouped generic response and no-response command handlers by operation and kept the same `Generic...CommandHandler` names with different generic arity.
- Added `TurtlePath.Automations` with profile and attribute descriptors for declarative handler automation.
- Added generated-style automation handler registration for create, update, delete, patch, get by id, get one, get many, and paged query flows.
- Added `IPatchAction<TEntity>` and a replaceable patch step so automated patch commands can apply request-owned changes.
- Updated the basic sample to use automation profiles for the recommended Customer happy path while retaining manual handlers for custom cases.
- Switched automation handler generation to DynaBee-backed generated handler types behind a replaceable generation abstraction.
- Added attribute-based automation examples to the basic sample.
- Added benchmarks comparing the same TurtlePath create flow through a manually written Pelican handler and a generated TurtlePath automation handler.
- Added CI package validation and NuGet release workflow support for Trusted Publishing.
- Added `TurtlePath.Analyzers` to warn when CId values from entities with different configured value types are compared or assigned.
- Included provider-specific adapter implementations: `AutoMapperAdapter`, `OctoMapAdapter`, `FluentValidationAdapter`, and `CrabalidatorAdapter`.
- Included builder extension types for EF Core and Sieve, keeping the public registration flow centered on `AddTurtlePath(...).Use...()` chaining.
- Included EF Core default registrations for `IStorageReaderAdapter` and `IStorageWriterAdapter` when `UseEntityFrameworkCore<TDbContext>()` is used.
- Included hook runner services behind `IHandlerHookRunner`.
- Included `NotFoundException` as an HTTP 404 `HttpException`.
- Added the TurtlePath package icon and made all source library projects explicitly packable NuGet projects.
- Configured the NuGet Trusted Publishing workflow to read the `NUGET_USER` repository variable from the repository owner.
