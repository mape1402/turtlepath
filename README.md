# TurtlePath

TurtlePath is a reusable .NET library for the Pelican handler foundation that previously lived inside the Elysium application template.

It packages the template's base command/query handlers, handler hook pipeline, validation and mapping adapters, storage abstractions, response/request primitives, custom identifier support, and Entity Framework helper configuration into a standalone library.

## Projects

- `src/TurtlePath`: reusable library.
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
services.AddBusiness(typeof(MyApplicationMarker).Assembly);
```

Then derive your Pelican handlers from the provided base handlers, for example `CreateCommandHandler<TRequest, TResponse, TEntity>`, `UpdateCommandHandler<TRequest, TResponse, TEntity>`, `DeleteCommandHandler<TRequest, TResponse, TEntity>`, `GetOneQueryHandler<TQuery, TValue, TEntity, TResponse>`, or `GetPagedInfoQueryHandler<TQuery, TEntity, TResponse>`.

## Extracted Template Surface

- Pelican command and query handler bases.
- Ordered before/after handler hooks.
- Storage reader/writer adapter contracts and default EF Core adapters.
- Mapper adapter backed by OctoMap.
- Validator adapter backed by Crabalidator.
- HTTP-style domain exceptions.
- `BaseEntity`, `IEntity<TId>`, `BaseRequest`, `BaseResponse`, and `PagedResponse<T>`.
- Configurable `CId` identifier, JSON converters, EF value generator, and base entity configuration.

## Build

```powershell
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release --no-build
```
