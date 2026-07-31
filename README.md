# TurtlePath

TurtlePath is a reusable .NET library for the Pelican handler foundation that previously lived inside the Elysium application template.

It packages the template's base command/query handlers, handler hook pipeline, validation and mapping adapters, storage abstractions, response/request primitives, custom identifier support, and Entity Framework helper configuration into a standalone library.

## Packages

The recommended Elysium stack is:

- `TurtlePath`: Pelican command/query handlers, hooks, request/response models, and application exceptions.
- `TurtlePath.Domain`: `BaseEntity`, `IEntity<TKey>`, and configurable `CId` identifiers.
- `TurtlePath.EntityFrameworkCore`: `BaseDbContext`, model conventions, `IDbContext`, and EF-backed storage adapters.
- `TurtlePath.OctoMap`: mapper adapter for the Elysium mapping stack.
- `TurtlePath.Crabalidator`: validator adapter for the Elysium validation stack.
- `TurtlePath.Sieve`: optional string-based filtering and sorting for query criteria.

Alternative adapters are available when a project needs them: `TurtlePath.AutoMapper` and `TurtlePath.FluentValidation`.

## Install

```powershell
dotnet add package TurtlePath
```

## Basic Usage

Install the focused packages your application actually uses. For example, a typical handler stack may use:

```powershell
dotnet add package TurtlePath
dotnet add package TurtlePath.EntityFrameworkCore
dotnet add package TurtlePath.Sieve
dotnet add package TurtlePath.OctoMap
dotnet add package TurtlePath.Crabalidator
```

Use one mapper adapter package and one validation adapter package. In Elysium projects, prefer `TurtlePath.OctoMap` and `TurtlePath.Crabalidator`.

Register TurtlePath and chain each implementation package from your application composition root:

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

`UseEntityFrameworkCore<TDbContext>()` maps your context to `IDbContext` and registers the default EF-backed `IStorageReaderAdapter` and `IStorageWriterAdapter`. Register your `DbContext` itself with the normal EF Core APIs:

```csharp
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
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

## Recommended Usage

Use `BaseEntity` for new entities. It gives every entity a `CId` identifier while allowing each application to decide how that identifier is generated and stored.

```csharp
public sealed class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
```

Responses should inherit from `BaseResponse` when they represent a `BaseEntity`:

```csharp
public sealed class CustomerResponse : BaseResponse
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
```

Create commands do not need to carry an id, so they can be plain Pelican requests:

```csharp
public sealed record CreateCustomerRequest(string Name, string Email)
    : IRequest<CustomerResponse>;
```

Update, patch, and delete commands should inherit from `BaseRequest`:

```csharp
public sealed class UpdateCustomerRequest : BaseRequest, IRequest<CustomerResponse>
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
```

Then derive Pelican handlers from the concise handler names. These are the recommended handlers for `BaseEntity` + `CId`:

```csharp
public sealed class CreateCustomerHandler
    : CreateCommandHandler<CreateCustomerRequest, CustomerResponse, Customer>
{
    public CreateCustomerHandler(IServiceProvider services) : base(services)
    {
    }
}
```

Queries follow the same convention:

```csharp
public sealed class GetCustomerByIdQuery : GetByIdQuery<Customer, CustomerResponse>
{
    public GetCustomerByIdQuery(CId id) : base(id)
    {
    }
}
```

The generic handlers are for legacy or specialized models only. They depend on `IEntity<TKey>`, `IBaseRequest<TKey>`, and `IBaseResponse<TKey>` without requiring `BaseEntity`:

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
    : GenericUpdateCommandHandler<UpdateLegacyCustomerRequest, LegacyCustomerResponse, LegacyCustomer, int>
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


