# TurtlePath

TurtlePath is a reusable .NET library for the Pelican handler foundation that previously lived inside the Elysium application template.

It packages the template's base command/query handlers, handler hook pipeline, validation and mapping adapters, storage abstractions, response/request primitives, custom identifier support, and Entity Framework helper configuration into a standalone library.

## Packages

The recommended Elysium stack is:

- `TurtlePath`: Pelican command/query handlers, hooks, request/response models, and application exceptions.
- `TurtlePath.Automations`: profile and attribute driven handler automation for standard TurtlePath flows.
- `TurtlePath.Domain`: `BaseEntity`, `IEntity<TKey>`, and configurable `CId` identifiers.
- `TurtlePath.EntityFrameworkCore`: `BaseDbContext`, model conventions, `IDbContext`, and EF-backed storage adapters.
- `TurtlePath.ExceptionHandling`: transport-neutral exception mapping to descriptors.
- `TurtlePath.ExceptionHandling.AspNetCore`: MVC exception filter, HTTP status mapping, and `ProblemDetails` responses.
- `TurtlePath.ExceptionHandling.Consumers`: exception boundaries for message consumers with complete/rethrow policies.
- `TurtlePath.ExceptionHandling.Workers`: exception boundaries for background services and one-shot workloads.
- `TurtlePath.Jobs`: standard one-shot Kubernetes jobs and recurring cron-style background jobs.
- `TurtlePath.OctoMap`: mapper adapter for the Elysium mapping stack.
- `TurtlePath.Crabalidator`: validator adapter for the Elysium validation stack.
- `TurtlePath.Sieve`: optional string-based filtering and sorting for query criteria.
- `TurtlePath.Analyzers`: optional compile-time checks for unsafe `CId` usage across entities with different configured identifier value types.

Alternative adapters are available when a project needs them: `TurtlePath.AutoMapper` and `TurtlePath.FluentValidation`.

## Install

```powershell
dotnet add package TurtlePath
```

## Basic Usage

Install the focused packages your application actually uses. For example, a typical handler stack may use:

```powershell
dotnet add package TurtlePath
dotnet add package TurtlePath.Automations
dotnet add package TurtlePath.EntityFrameworkCore
dotnet add package TurtlePath.ExceptionHandling
dotnet add package TurtlePath.ExceptionHandling.AspNetCore
dotnet add package TurtlePath.ExceptionHandling.Consumers
dotnet add package TurtlePath.ExceptionHandling.Workers
dotnet add package TurtlePath.Jobs
dotnet add package TurtlePath.Sieve
dotnet add package TurtlePath.OctoMap
dotnet add package TurtlePath.Crabalidator
dotnet add package TurtlePath.Analyzers
```

Use one mapper adapter package and one validation adapter package. In Elysium projects, prefer `TurtlePath.OctoMap` and `TurtlePath.Crabalidator`.

Analyzer packages should stay private to the project that consumes them:

```xml
<PackageReference Include="TurtlePath.Analyzers" Version="..." PrivateAssets="all" />
```

Register Pelican, the provider libraries, TurtlePath, and each implementation package from your application composition root:

```csharp
services.AddPelican(typeof(MyApplicationMarker).Assembly);

services.AddCrabalidator(typeof(MyApplicationMarker).Assembly);

services.AddOctoMap(registration =>
{
    registration.Options.EnableRuntimeImplicitMaps = false;
    registration.Options.DuplicateMapPolicy = DuplicateMapPolicy.Throw;
    registration.AddMaps(typeof(MyApplicationMarker).Assembly);
});

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
    .UseAutomations(typeof(MyApplicationMarker).Assembly)
    .UseOctoMap()
    .UseCrabalidator()
    .UseSieve()
    .UseEntityFrameworkCore<AppDbContext>(options => options with
    {
        ApplyConfigurations = true,
        ApplyBaseEntityConventions = true,
        ApplyCIdConverters = true,
        ConfigurationAssemblies = [typeof(MyPersistenceMarker).Assembly]
    });
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

Use `TurtlePath.Automations` for standard create, update, delete, patch, and query happy paths. Write DTOs, entities, mappings, validators, and an automation profile; Pelican still resolves the final handlers through `mediator.Send(...)`.

```csharp
public sealed class CommerceAutomationProfile : TurtlePathAutomationProfile
{
    public override void Configure(ITurtlePathAutomationBuilder builder)
    {
        builder.For<Customer>()
            .ToCreate<CreateCustomerRequest, CustomerResponse>()
            .ToUpdate<UpdateCustomerRequest, CustomerResponse>()
            .ToPatch<PatchCustomerEmailRequest, CustomerResponse>()
            .ToGetById<GetCustomerByIdQuery, CustomerResponse>()
            .ToGetPaged<GetCustomersPageQuery, CustomerResponse>(query => query.DefaultSort("Name"));
    }
}
```

The recommended mapper adapter is OctoMap. Keep mappings explicit so the handler pipeline is predictable:

```csharp
public sealed class CommerceMappingProfile : OctoMapProfile
{
    public override void Configure(IOctoMapConfigurationBuilder builder)
    {
        builder.CreateMap<CreateCustomerRequest, Customer>();
        builder.CreateMap<UpdateCustomerRequest, Customer>();
        builder.CreateMap<Customer, CustomerResponse>();

        builder.CreateMap<CatalogItem, DeletedResourceResponse>()
            .ForMember(x => x.Resource, x => x.MapFrom(_ => nameof(CatalogItem)));
    }
}
```

The recommended validator adapter is Crabalidator. TurtlePath calls `IValidatorAdapter` from its command steps, so validators stay outside handlers:

```csharp
public sealed class CreateCustomerRequestValidator : CrabValidator<CreateCustomerRequest>
{
    public CreateCustomerRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Email).NotEmpty().MaximumLength(120).Must(value => value.Contains('@'));
    }
}
```

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

Patch commands used by automations should also implement `IPatchAction<TEntity>`:

```csharp
public sealed class PatchCustomerEmailRequest : BaseRequest, IRequest<CustomerResponse>, IPatchAction<Customer>
{
    public string Email { get; set; } = string.Empty;

    public ValueTask PatchAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        entity.Email = Email.Trim().ToLowerInvariant();
        return ValueTask.CompletedTask;
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

Manual handlers remain the extension point when a flow has special behavior. The virtual handler methods still exist for local overrides, while the default implementations delegate to replaceable flow steps from DI.

## Automations

Automations can be declared in profiles, which is the preferred style for application-wide configuration:

```csharp
public sealed class CommerceAutomationProfile : TurtlePathAutomationProfile
{
    public override void Configure(ITurtlePathAutomationBuilder builder)
    {
        builder.For<Customer>()
            .ToCreate<CreateCustomerRequest, CustomerResponse>()
            .ToUpdate<UpdateCustomerRequest, CustomerResponse>()
            .ToPatch<PatchCustomerEmailRequest, CustomerResponse>()
            .ToGetById<GetCustomerByIdQuery, CustomerResponse>()
            .ToGetPaged<GetCustomersPageQuery, CustomerResponse>(query => query.DefaultSort("Name"));
    }
}
```

Small or local cases can use attributes:

```csharp
[CreateAutomation(typeof(CatalogItem), typeof(CatalogItemResponse))]
public sealed record CreateCatalogItemRequest(string Sku, string Name, decimal Price)
    : IRequest<CatalogItemResponse>;
```

Automations generate concrete Pelican handlers with DynaBee and register them in DI. At runtime they execute the same TurtlePath handler base classes and steps used by manually written handlers.

## Exception Handling

`TurtlePath.ExceptionHandling` keeps exception rules transport-neutral. Applications map exceptions once into an `ExceptionDescriptor`; target adapters decide how to project that descriptor to HTTP, consumers, workers, or jobs.

Use profiles when the exception catalog starts growing:

```csharp
public sealed class CommerceExceptionHandlingProfile : ExceptionHandlingProfile
{
    public override void Configure(ExceptionHandlingOptionsBuilder builder)
    {
        builder.For<ValidationException>(
            _ => ExceptionKind.Validation,
            _ => "validation",
            ex => ex.Errors);

        builder.For<PaymentDeclinedException>(
            new ExceptionKind("payment_declined"),
            ex => ex.Message);
    }
}
```

Register the profile and adapters from the composition root:

```csharp
services.AddExceptionHandlingProfile<CommerceExceptionHandlingProfile>();

services.AddTurtlePathAspNetCoreExceptionHandling(builder =>
{
    builder.Map(new ExceptionKind("payment_declined"), StatusCodes.Status402PaymentRequired);
});

services.AddTurtlePathConsumerExceptionHandling(builder =>
{
    builder.RethrowWhen((descriptor, _) => descriptor.Kind != ExceptionKind.Validation);
});

services.AddTurtlePathWorkerExceptionHandling(builder =>
{
    builder.RethrowWhen(descriptor => descriptor.Kind == ExceptionKind.Transient);
    builder.Return(descriptor => $"handled:{descriptor.Code}");
});
```

ASP.NET Core can use the packaged MVC filter:

```csharp
services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});
```

Consumers can wrap message processing without depending on a specific broker package:

```csharp
await consumerExceptionBoundary.RunAsync(
    message,
    async (message, cancellationToken) =>
    {
        await mediator.Send(message, cancellationToken);
    },
    new ConsumerExceptionContext
    {
        MessageId = messageId,
        CorrelationId = correlationId,
        DeliveryCount = deliveryCount
    },
    cancellationToken);
```

Workers and Kubernetes-style one-shot jobs use the background boundary. By default it rethrows handled exceptions so the host or CronJob can observe the failure.

## Jobs

`TurtlePath.Jobs` provides a standard execution path for one-shot jobs and recurring cron-style background jobs. A job only implements the work:

```csharp
public sealed class ImportCustomersJob : TurtlePathJob
{
    public override Task ExecuteAsync(TurtlePathJobContext context, CancellationToken cancellationToken)
    {
        // Import customers here.
        return Task.CompletedTask;
    }
}
```

For Kubernetes CronJobs or console workloads, register one or more one-shot jobs and run the manager. The manager waits for every job and returns when the whole batch is done:

```csharp
services.AddTurtlePathJobs(options =>
{
    options.ExecutionMode = TurtlePathJobExecutionMode.Parallel;
    options.MaxDegreeOfParallelism = 4;
    options.Retries = 2;
    options.RetryDelay = TimeSpan.FromSeconds(10);
    options.FailureBehavior = TurtlePathJobFailureBehavior.Rethrow;
})
.AddJob<ImportCustomersJob>("import-customers")
.AddJob<ImportInvoicesJob>("import-invoices");

await provider.RunTurtlePathJobsAsync();
```

For long-running services, register multiple recurring jobs. TurtlePath adds one hosted service that manages independent loops for every registered cron job:

```csharp
services.AddTurtlePathJobs()
    .AddCronJob<RefreshCatalogJob>(options =>
    {
        options.EveryMinutes(30);
        options.Retries = 3;
        options.FailureBehavior = TurtlePathJobFailureBehavior.Continue;
    })
    .AddCronJob<SyncCustomersJob>(options =>
    {
        options.EveryHours(6);
        options.RunOnStart = true;
        options.FailureBehavior = TurtlePathJobFailureBehavior.StopHost;
    });
```

Each execution creates a fresh DI scope and runs through `IBackgroundExceptionBoundary`, so retries, reporting, and failure behavior are consistent with the rest of TurtlePath exception handling.

## Analyzers

`TurtlePath.Analyzers` protects the main sharp edge of scalar `CId`: two entities can expose `CId` while masking different value types. For example, a clean `Customer` may use the default `Guid` configuration while a legacy `Invoice` stores its id as `int`.

```csharp
if (customer.Id == invoice.Id) // TP0001
{
}

customer.Id = invoice.Id; // TP0002
```

The analyzer uses the CId registrations it can see in source, such as `UseCId<Guid, string>()` and `UseCIdFor<LegacyInvoice, int, int>()`. It only reports when it can infer both entity id value types.


