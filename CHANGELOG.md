# Changelog

All notable changes to TurtlePath will be documented in this file.

## [v1.0.0] - Unreleased

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

### Changed

- Cleaned the public API surface by renaming concrete adapter implementations to provider-specific names: `AutoMapperAdapter`, `OctoMapAdapter`, `FluentValidationAdapter`, and `CrabalidatorAdapter`.
- Renamed builder extension types for EF Core and Sieve, keeping the public registration flow centered on `AddTurtlePath(...).Use...()` chaining.
- Made EF Core register `IStorageReaderAdapter` and `IStorageWriterAdapter` by default when `UseEntityFrameworkCore<TDbContext>()` is used.
- Hid the default hook runner implementation behind `IHandlerHookRunner`.
- Made `NotFoundException` inherit `HttpException` with HTTP 404 semantics.
- Added the TurtlePath package icon and made all source library projects explicitly packable NuGet projects.
- Removed the default NuGet Trusted Publishing user from the release workflow; the `NUGET_USER` repository variable must be configured by the repository owner.
