# TurtlePath

TurtlePath is a reusable .NET library for the Pelican handler foundation that previously lived inside the Elysium application template.

It packages the template's base command/query handlers, handler hook pipeline, validation and mapping adapters, storage abstractions, response/request primitives, custom identifier support, and Entity Framework helper configuration into a standalone library.

## Projects

- `src/TurtlePath`: compatibility package that composes the extracted stack.
- `src/TurtlePath.Abstractions`: provider-neutral mapping, validation, and persistence contracts.
- `src/TurtlePath.Identifier`: opaque identifiers, single-part IDs, composite IDs, and identifier definitions.
- `src/TurtlePath.Identifier.EntityFrameworkCore`: EF Core integration for identifier generation.
- `src/TurtlePath.Domain`: entity contracts and domain base types.
- `src/TurtlePath.Application`: Pelican handler bases, hooks, request/response models, and application errors.
- `src/TurtlePath.EntityFrameworkCore`: EF Core context abstraction, storage adapters, and entity configuration helper.
- `src/TurtlePath.OctoMap`: OctoMap mapper adapter.
- `src/TurtlePath.Crabalidator`: Crabalidator validator adapter.
- `src/TurtlePath.Sieve`: Sieve criteria adapter.
- `src/TurtlePath.AspNetCore`: JSON and ASP.NET Core integration helpers.
- `src/TurtlePath.Swagger`: OpenAPI schema helpers.
- `tests/TurtlePath.Tests`: unit tests for extracted primitives and registration behavior.
- `samples/TurtlePath.Samples.Basic`: small usage-oriented sample.
- `benchmarks/TurtlePath.Benchmarks`: BenchmarkDotNet entry point placeholder following the OctoMap repository shape.

## Install

```powershell
dotnet add package TurtlePath
```

## Basic Usage

Register the base services from your application composition root:

```csharp
services.AddTurtlePath(options => options.AddApplicationAssemblies(typeof(MyApplicationMarker).Assembly));
```

Prefer explicit packages for new applications:

```csharp
services.AddTurtlePathSieve();
services.AddTurtlePath(options => options.AddApplicationAssemblies(typeof(MyApplicationMarker).Assembly));
```

Then derive your Pelican handlers from the provided base handlers, for example `CreateCommandHandler<TRequest, TResponse, TEntity>`, `UpdateCommandHandler<TRequest, TResponse, TEntity>`, `DeleteCommandHandler<TRequest, TResponse, TEntity>`, `GetOneQueryHandler<TQuery, TValue, TEntity, TResponse>`, or `GetPagedInfoQueryHandler<TQuery, TEntity, TResponse>`.

## Extracted Template Surface

- Pelican command and query handler bases.
- Ordered before/after handler hooks.
- Storage reader/writer adapter contracts and default EF Core adapters.
- Mapping contract plus an OctoMap-backed adapter.
- Validation contract plus a Crabalidator-backed adapter.
- Application errors separated from ASP.NET Core HTTP exceptions.
- `BaseEntity`, `IEntity<TId>`, `BaseRequest`, `BaseResponse`, and `PagedResponse<T>`.
- Configurable `CId` identifier, JSON converters, EF identifier value generator, and base entity configuration.

The root `TurtlePath` package remains as a compatibility composition package. New consumers should prefer the smallest focused packages that match their stack.

## Build

```powershell
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release --no-build
```
