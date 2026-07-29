# TurtlePath Roadmap

This roadmap describes how TurtlePath should evolve from the extracted template foundation into a layered, extensible set of libraries.

## Goals

- Preserve the template's architectural separation between domain, application, and infrastructure.
- Keep each package focused, replaceable, and easy to version.
- Avoid leaking infrastructure dependencies into domain or application code.
- Make identifiers opaque to application code while supporting different database key shapes.
- Keep first-party adapters for the current Elysium stack: Pelican, EF Core, OctoMap, Crabalidator, and Sieve.

## Package Shape

### TurtlePath.Identifier

Opaque identity primitives.

- `CId`
- `CIdPart`
- `CIdDefinition`
- `ICIdDefinitionRegistry`
- `ICIdFactory`
- parsing and formatting contracts
- equality and comparison semantics

No EF Core, JSON, ASP.NET, Swagger, mapping, validation, or mediator dependencies.

Design direction:

- `CId` should support one or more named parts.
- Single-part IDs remain ergonomic through helpers such as `CId.From(value)`.
- Composite IDs compare by stable part names and values.
- ID generation should be resolved by context, not by one global static metadata object.

### TurtlePath.Domain

Domain primitives only.

- `IEntity<TKey>`
- `IEntity`
- `BaseEntity<TKey>`
- optional `BaseEntity : BaseEntity<CId>`

Dependencies:

- `TurtlePath.Identifier` only if `CId` is the preferred default identity type.

No handlers, DI, EF Core, validators, mappers, storage adapters, or mediator contracts.

### TurtlePath.Application

Application handler foundation.

- Pelican command/query handler base classes
- hook contexts
- hook interfaces
- handler hook runner
- application request/response primitives
- application exceptions
- adapter contracts:
  - `IMapperAdapter`
  - `IValidatorAdapter`
  - `IStorageReaderAdapter`
  - `IStorageWriterAdapter`

Dependencies:

- `Pelican.Mediator`
- `TurtlePath.Domain`
- `Microsoft.Extensions.DependencyInjection.Abstractions`, only for DI helpers and current handler construction style.

No EF Core, OctoMap, Crabalidator, Sieve, JSON, or Swagger dependencies.

### TurtlePath.Abstractions

Shared contracts that are not tied to EF Core or any mapping/validation implementation.

- unit of work abstractions
- read/write set abstractions
- query criteria abstractions
- paging contracts
- mapper adapter contract
- validator adapter contract
- validation error contract

This package should avoid exposing `DbContext`, `DbSet`, `IEntityTypeConfiguration`, `DatabaseFacade`, or `ChangeTracker`.

### TurtlePath.EntityFrameworkCore

EF Core adapter package.

- EF-backed storage reader/writer adapters
- EF-specific `IDbContext` if still useful
- entity configurations
- CId value conversion integration through `TurtlePath.Identifier.EntityFrameworkCore`
- DI extensions for EF-backed TurtlePath services

Dependencies:

- `Microsoft.EntityFrameworkCore`
- `TurtlePath.Domain`
- `TurtlePath.Identifier`
- `TurtlePath.Identifier.EntityFrameworkCore`
- `TurtlePath.Abstractions`

No OctoMap or Crabalidator dependency unless a separate convenience package intentionally composes them.

### TurtlePath.Identifier.EntityFrameworkCore

EF Core integration for identifiers.

- `ValueConverter<CId, TValue>`
- support for client-generated and store-generated IDs
- property configuration helpers
- composite key configuration helpers

Examples to support:

```csharp
builder.UseCId<Customer>(x => x.Id, id =>
{
    id.HasValueType<int>();
    id.ValueGeneratedOnAdd();
});

builder.UseCId<Order>(x => x.Id, id =>
{
    id.HasValueType<Guid>();
    id.HasClientFactory(Guid.NewGuid);
});

builder.UseCompositeCId<OrderLine>(id =>
{
    id.HasPart(x => x.OrderId, "OrderId", Guid.Parse);
    id.HasPart(x => x.LineNumber, "LineNumber", int.Parse);
});
```

### TurtlePath.OctoMap

Mapping adapter package.

- `OctoMapMapperAdapter`
- DI extensions for registering the mapper adapter

Dependencies:

- `OctoMap`
- `TurtlePath.Abstractions`

### TurtlePath.Crabalidator

Validation adapter package.

- `CrabalidatorValidatorAdapter`
- DI extensions for registering the validator adapter

Dependencies:

- `Crabalidator`
- `TurtlePath.Abstractions`

### TurtlePath.Sieve

Query filtering and sorting adapter package.

- Sieve-backed query criteria translation
- Sieve processor registration helpers

Dependencies:

- `Sieve`
- `TurtlePath.Abstractions`

### TurtlePath.AspNetCore

ASP.NET integration package.

- JSON converter registration helpers
- model binding helpers for `CId`
- route value parsing
- exception-to-response helpers if desired

No EF Core dependency.

### TurtlePath.Swagger

OpenAPI integration package.

- schema filters for `CId`
- schema filters for composite IDs

Dependencies:

- Swashbuckle or other OpenAPI package only here.

## Migration Plan

### Phase 1: Stabilize The Current Extraction

- Keep the current `TurtlePath` package compiling.
- Add a compatibility namespace map if needed.
- Expand tests around extracted handler behavior.
- Document the current behavior before splitting projects.
- Mark infrastructure-heavy APIs as candidates for relocation.

Done criteria:

- Build and tests pass for `net8.0`, `net9.0`, and `net10.0`.
- The README states which APIs are temporary compatibility surface.

### Phase 2: Extract Identifier

- Create `src/TurtlePath.Identifier`.
- Move `CId` into the identifier package.
- Replace `CIdMetadata` global state with registry/factory abstractions.
- Support single-part IDs first.
- Design `CId` internally as part-based so composite IDs can be added without rewriting equality.
- Add tests for:
  - equality
  - formatting
  - parsing
  - different underlying types in the same process
  - store-generated vs client-generated definitions

Done criteria:

- `TurtlePath.Identifier` has no infrastructure dependencies.
- Multiple ID definitions can coexist.

### Phase 3: Split Domain

- Create `src/TurtlePath.Domain`.
- Move `IEntity<TKey>`, `IEntity`, and base entity types.
- Decide whether `BaseEntity` defaults to `CId`.
- Update application handlers to depend on domain contracts.

Done criteria:

- A consumer domain project can reference `TurtlePath.Domain` without getting Pelican, EF Core, OctoMap, Crabalidator, or Sieve.

### Phase 4: Split Application

- Create `src/TurtlePath.Application`.
- Move handler bases, hooks, contexts, request/response primitives, exceptions, and adapter contracts.
- Rename `AddBusiness` to a TurtlePath-specific registration API.
- Keep old extension names temporarily only if compatibility matters.

Preferred API:

```csharp
services.AddTurtlePathApplication(options =>
{
    options.AddHooksFromAssemblyContaining<AppMarker>();
});
```

Done criteria:

- `TurtlePath.Application` does not reference EF Core, OctoMap, Crabalidator, or Sieve.

### Phase 5: Consolidate Abstractions

- Create `src/TurtlePath.Abstractions`.
- Move storage, mapping, and validation contracts that do not require implementations.
- Remove `BaseEntity` assumptions where practical.
- Prefer key-agnostic abstractions or `IEntity<TKey>` constraints.

Done criteria:

- Application code can depend on persistence contracts without referencing EF Core.

### Phase 6: Split EF Core

- Create `src/TurtlePath.EntityFrameworkCore`.
- Move EF-specific storage adapters.
- Move EF-specific `IDbContext`.
- Move entity configurations.
- Wire EF configuration against `TurtlePath.Identifier.EntityFrameworkCore`.

Done criteria:

- No EF types appear in `TurtlePath.Application`, `TurtlePath.Domain`, or `TurtlePath.Identifier`.

### Phase 7: Split Mapping And Validation Adapters

- Create `src/TurtlePath.OctoMap`.
- Move `MapperAdapter`.
- Create `src/TurtlePath.Crabalidator`.
- Move `ValidatorAdapter`.
- Add tests proving consumers can replace either adapter.

Done criteria:

- The application package exposes contracts only.
- OctoMap and Crabalidator are optional package choices.

### Phase 8: Split Filtering

- Create `src/TurtlePath.Sieve`.
- Move Sieve-specific filtering and sorting behavior.
- Keep core query criteria independent from Sieve syntax.

Done criteria:

- EF Core adapter can read generic criteria without requiring Sieve.
- Sieve is opt-in.

### Phase 9: Add ASP.NET And Swagger Integrations

- Create `src/TurtlePath.AspNetCore`.
- Create `src/TurtlePath.Swagger`.
- Move JSON converters, model binding, and schema filters out of identifier/domain.

Done criteria:

- API projects can opt into web concerns explicitly.
- Domain and identifier packages remain web-framework agnostic.

### Phase 10: Samples, Docs, And Package Release

- Add sample projects:
  - simple GUID IDs
  - int database-generated IDs
  - mixed ID types
  - composite key example
  - EF Core + OctoMap + Crabalidator full stack
- Expand README per package.
- Add package dependency diagrams.
- Update CI to build, test, and pack all packages.
- Update release workflow to publish all package artifacts.

Done criteria:

- Every package has a short README section.
- CI validates all target frameworks.
- NuGet packing works for all packages.

## Dependency Rules

Allowed dependency direction:

```text
Identifier
Domain -> Identifier
Identifier.EntityFrameworkCore -> Identifier
Abstractions -> Domain
Application -> Domain, Identifier, Abstractions
EntityFrameworkCore -> Domain, Identifier, Identifier.EntityFrameworkCore, Abstractions
OctoMap -> Abstractions
Crabalidator -> Abstractions
Sieve -> Abstractions
AspNetCore -> Identifier, Application
Swagger -> Identifier, AspNetCore
```

Forbidden:

- Domain depending on Application.
- Domain depending on EF Core, Pelican, OctoMap, Crabalidator, Sieve, ASP.NET, or Swagger.
- Identifier depending on EF Core, JSON, ASP.NET, or Swagger.
- Application depending on EF Core, OctoMap, Crabalidator, or Sieve implementations.
- EF Core package depending directly on OctoMap or Crabalidator.

## Open Decisions

- Should `BaseEntity` default to `CId`, or should consumers always choose `BaseEntity<TKey>`?
- Should `CId` expose raw values publicly, or only through controlled typed accessors?
- Should composite ID serialization preserve declaration order, part names, or both?
- Should `CId` be a `readonly record struct` or a custom readonly struct with tighter allocation control?
- Should compatibility APIs from the first extraction remain until `v2.0.0`, or can they be removed before first release?

## Near-Term Next Step

Start with `TurtlePath.Identifier`, because it is the most foundational piece and directly affects domain, EF Core, JSON, routing, and composite-key support.
