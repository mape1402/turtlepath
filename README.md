# TurtlePath

TurtlePath is a reusable .NET library for the Pelican handler foundation that previously lived inside the Elysium application template.

It packages the template's base command/query handlers, handler hook pipeline, validation and mapping adapters, storage abstractions, response/request primitives, custom identifier support, and Entity Framework helper configuration into a standalone library.

## Projects

- `src/TurtlePath.Abstractions`: provider-neutral mapping, validation, and persistence contracts.
- `src/TurtlePath.Domain`: opaque identifiers, entity contracts, and domain base types.
- `src/TurtlePath`: Pelican handler bases, hooks, request/response models, and application errors.
- `src/TurtlePath.EntityFrameworkCore`: EF Core context abstraction, base DbContext conventions, and storage adapters.
- `src/TurtlePath.OctoMap`: OctoMap mapper adapter.
- `src/TurtlePath.AutoMapper`: AutoMapper mapper adapter.
- `src/TurtlePath.Crabalidator`: Crabalidator validator adapter.
- `src/TurtlePath.FluentValidation`: FluentValidation validator adapter.
- `src/TurtlePath.Sieve`: Sieve criteria adapter.
- `tests/TurtlePath.Tests`: unit tests for extracted primitives and registration behavior.
- `samples/TurtlePath.Samples.Basic`: small usage-oriented sample.
- `benchmarks/TurtlePath.Benchmarks`: BenchmarkDotNet entry point placeholder following the OctoMap repository shape.

## Install

```powershell
dotnet add package TurtlePath
```

## Basic Usage

Install the focused packages your application actually uses. For example, a typical handler stack may use:

```powershell
dotnet add package TurtlePath
dotnet add package TurtlePath.EntityFrameworkCore
dotnet add package TurtlePath.OctoMap
dotnet add package TurtlePath.AutoMapper
dotnet add package TurtlePath.Crabalidator
dotnet add package TurtlePath.FluentValidation
dotnet add package TurtlePath.Sieve
```

Register each implementation package from your application composition root:

```csharp
services
    .AddTurtlePath(typeof(MyApplicationMarker).Assembly)
    .UseCId<Guid, string>(config =>
    {
        config.DefaultFactory = () => CId.From(Guid.NewGuid());
        config.ConvertToDb = id => id.ToString();
        config.ConvertFromDb = value => CId.Parse(value);
        config.JsonConverter = value => CId.Parse(value);
        config.NullableJsonConverter = value => string.IsNullOrWhiteSpace(value) ? null : CId.Parse(value);
        config.ParseFunction = value => CId.From(Guid.Parse(value));
        config.ToByteArrayFunction = value => value.ToByteArray();
    })
    .UseCIdProfile<LegacyIdentifierProfile>()
    .UseEntityFrameworkCore<AppDbContext>(options => options with
    {
        ApplyConfigurations = true,
        ApplyBaseEntityConventions = true,
        ApplyCIdConverters = true,
        ConfigurationAssemblies = [typeof(MyPersistenceMarker).Assembly]
    })
    .UseOctoMap()
    .UseCrabalidator()
    .UseSieve();
```

`UseCId<TValue, TDbValue>()` configures the default identifier used by every entity. Put legacy or mixed-schema overrides in a profile:

```csharp
public sealed class LegacyIdentifierProfile : CIdProfile
{
    public override void Configure(CIdProfileBuilder builder)
    {
        builder.UseCIdFor<LegacyCustomer, int, int>(config =>
        {
            config.DefaultFactory = () => CId.From(0);
            config.ConvertToDb = id => id.Cast<int>();
            config.ConvertFromDb = value => CId.From(value);
            config.JsonConverter = value => CId.From(int.Parse(value));
            config.NullableJsonConverter = value => string.IsNullOrWhiteSpace(value) ? null : CId.From(int.Parse(value));
            config.ParseFunction = value => CId.From(int.Parse(value));
            config.ToByteArrayFunction = value => BitConverter.GetBytes(value);
        });
    }
}
```

Use `UseCIdProfiles(typeof(MyPersistenceMarker).Assembly)` when you prefer assembly discovery instead of registering each profile explicitly.

An EF Core context can receive the registered TurtlePath options through DI:

```csharp
public sealed class AppDbContext : BaseDbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        TurtlePathDbContextOptions turtlePathOptions,
        IEnumerable<ITurtlePathModelConvention> modelConventions)
        : base(options, turtlePathOptions, modelConventions)
    {
    }
}
```

Then derive your Pelican handlers from the provided base handlers. The short forms target the recommended TurtlePath domain path, where entities inherit from `BaseEntity` and use `CId`:

```csharp
public sealed class CreateCustomerHandler
    : CreateCommandHandler<CreateCustomerRequest, CustomerResponse, Customer>
{
    public CreateCustomerHandler(IServiceProvider services) : base(services)
    {
    }
}
```

For legacy or specialized models, use the explicit key overloads. These depend only on `IEntity<TKey>`, `IBaseRequest<TKey>`, and `IBaseResponse<TKey>`:

```csharp
public sealed class UpdateLegacyCustomerRequest : IBaseRequest<int>, IRequest<LegacyCustomerResponse>
{
    public int Id { get; set; }

    public string Name { get; set; }
}

public sealed class LegacyCustomer : IEntity<int>
{
    public int Id { get; set; }

    public string Name { get; set; }
}

public sealed class LegacyCustomerResponse : IBaseResponse<int>
{
    public int Id { get; set; }

    public string Name { get; set; }
}

public sealed class UpdateLegacyCustomerHandler
    : UpdateCommandHandler<UpdateLegacyCustomerRequest, LegacyCustomerResponse, LegacyCustomer, int>
{
    public UpdateLegacyCustomerHandler(IServiceProvider services) : base(services)
    {
    }
}
```

## Extracted Template Surface

- Pelican command and query handler bases.
- Ordered before/after handler hooks.
- Storage reader/writer adapter contracts and default EF Core adapters.
- Mapping contract plus OctoMap-backed and AutoMapper-backed adapters.
- Validation contract plus Crabalidator-backed and FluentValidation-backed adapters.
- Application exceptions used by the handler base classes.
- `BaseEntity`, `IEntity<TId>`, `BaseRequest`, `BaseResponse`, and `PagedResponse<T>`.
- Configurable `CId` identifier definitions, per-entity identifier overrides, identifier JSON converters, and configurable EF Core base DbContext conventions.

## Build

```powershell
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release --no-build
```


