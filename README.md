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
- `TurtlePath.EventSourcing`: Krackend event sourcing bridge for TurtlePath command handler hooks.
- `TurtlePath.Jobs`: standard one-shot Kubernetes jobs and recurring cron-style background jobs.
- `TurtlePath.Testing`: test host, delegate adapters, and in-memory storage for TurtlePath handler and automation tests.
- `TurtlePath.Testing.EntityFrameworkCore`: SQLite-backed integration testing helpers for EF Core TurtlePath applications.
- `TurtlePath.Testing.EventSourcing`: event stream assertion helpers for TurtlePath event sourcing tests.
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
dotnet add package TurtlePath.EventSourcing
dotnet add package TurtlePath.Jobs
dotnet add package TurtlePath.Testing
dotnet add package TurtlePath.Testing.EntityFrameworkCore
dotnet add package TurtlePath.Testing.EventSourcing
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

Testing packages should normally stay in test projects:

```xml
<PackageReference Include="TurtlePath.Testing" Version="..." PrivateAssets="all" />
<PackageReference Include="TurtlePath.Testing.EntityFrameworkCore" Version="..." PrivateAssets="all" />
<PackageReference Include="TurtlePath.Testing.EventSourcing" Version="..." PrivateAssets="all" />
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

## Testing

`TurtlePath.Testing` gives tests the same composition shape as the application without forcing a mocking framework. The test host registers TurtlePath, delegate mapper and validator adapters, an in-memory storage adapter, and optional Pelican dispatch.

Use it for direct handler unit tests when you want to avoid wiring every dependency by hand:

```csharp
await using var host = await TurtlePathTestHost
    .Create()
    .WithMap<CreateCustomerRequest, Customer>(request => new Customer
    {
        Name = request.Name,
        Email = request.Email
    })
    .WithMap<Customer, CustomerResponse>(customer => new CustomerResponse
    {
        Id = customer.Id,
        Name = customer.Name,
        Email = customer.Email
    })
    .WithValidRequest<CreateCustomerRequest>()
    .BuildAsync();

var handler = new CreateCustomerCommandHandler(host.Services);

var response = await handler.Handle(new CreateCustomerRequest("Ada", "ada@example.com"));

Assert.True(host.Store<Customer>().Contains(customer => customer.Email == "ada@example.com"));
```

Call `TraceHooks()` when a test needs to assert TurtlePath hook stage execution through `HookTrace`.

Use Pelican when the test should exercise the same dispatch path as production:

```csharp
await using var host = await TurtlePathTestHost
    .Create()
    .UsePelican(typeof(CreateCustomerCommandHandler).Assembly)
    .WithMap<CreateCustomerRequest, Customer>(request => new Customer { Name = request.Name })
    .WithMap<Customer, CustomerResponse>(customer => new CustomerResponse { Id = customer.Id, Name = customer.Name })
    .BuildAsync();

var response = await host.SendAsync(new CreateCustomerRequest("Ada", "ada@example.com"));
```

Automations should be tested as integration flows because the application does not own a concrete handler class:

```csharp
await using var host = await TurtlePathTestHost
    .Create()
    .UseAutomations(typeof(CustomerAutomationProfile).Assembly)
    .WithMap<CreateCustomerRequest, Customer>(request => new Customer { Name = request.Name })
    .WithMap<Customer, CustomerResponse>(customer => new CustomerResponse { Id = customer.Id, Name = customer.Name })
    .BuildAsync();

var response = await host.SendAsync(new CreateCustomerRequest("Ada", "ada@example.com"));
```

The core testing package stays storage-provider neutral. Use it for fast unit and lightweight integration tests; use provider-specific testing packages when a test must verify real infrastructure behavior such as EF Core SQLite persistence.

Use `TurtlePath.Testing.EntityFrameworkCore` when the test should prove real EF behavior:

```csharp
await using var host = await TurtlePathTestHost
    .Create()
    .UseAutomations(typeof(CustomerAutomationProfile).Assembly)
    .UseSqliteDbContext<AppDbContext>(options => options with
    {
        ConfigurationAssemblies = [typeof(AppDbContext).Assembly]
    })
    .WithMap<CreateCustomerRequest, Customer>(request => new Customer { Name = request.Name })
    .WithMap<Customer, CustomerResponse>(customer => new CustomerResponse { Id = customer.Id, Name = customer.Name })
    .BuildAsync();

await host.CreateSchemaAsync<AppDbContext>();

var response = await host.SendAsync(new CreateCustomerRequest("Ada", "ada@example.com"));
```

Use `UseExceptionHandling()`, `HandleException(...)`, `UseJobs()`, `WithJob<TJob>()`, and `RunJobsAsync()` for TurtlePath exception and job scenarios.

Use `TurtlePath.Testing.EventSourcing` when a command should append events:

```csharp
var events = await host.ReadEventStreamAsync("customers", customerId);

Assert.Contains(events, item => item.EventType == "customer-created");
```

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

## Event Sourcing

`TurtlePath.EventSourcing` connects TurtlePath command handlers to Krackend event sourcing through `IAfterSaveHook<TRequest, TEntity>`. The handler saves the entity first, then the hook resolves the Krackend stream, maps `request + entity` to one or more event payloads, and appends them to `IEventStore`.

Use profiles so event mappings do not grow inside dependency registration:

```csharp
public sealed class CommerceEventSourcingProfile : IEventSourcingProfile
{
    public void Configure(IEventSourcingProfileBuilder builder)
    {
        builder.For<CreateCustomerRequest, Customer>()
            .ToEvent<CustomerCreated>(options => options.UseExpectedVersion(ExpectedVersion.NoStream))
            .ToEvent<CustomerAuditRegistered>(options => options.UseExpectedVersion(ExpectedVersion.NoStream));

        builder.For<UpdateCustomerRequest, Customer>()
            .ToEvent<CustomerUpdated>();
    }
}
```

Commands use Krackend stream contracts:

```csharp
[EventStream("customers")]
public sealed record CreateCustomerRequest(string CustomerId, string Name, string Email)
    : IRequest<CustomerResponse>, IEventStreamCommand
{
    public string StreamId => CustomerId;
}
```

Register TurtlePath, the mapper adapter, and the profile from the composition root:

```csharp
services
    .AddTurtlePath(typeof(MyApplicationMarker).Assembly)
    .UseOctoMap()
    .UseEventSourcingProfile<CommerceEventSourcingProfile>();
```

Event payloads are mapped through TurtlePath's `IMapperAdapter`. For generated ids, resolve the stream from the saved entity and project the hook context into a small mapper source:

```csharp
public sealed class CommerceEventSourcingProfile : IEventSourcingProfile
{
    public void Configure(IEventSourcingProfileBuilder builder)
    {
        builder.For<CreateCustomerRequest, Customer>()
            .UseStream("customers", context => context.Entity.Id.ToString())
            .ToEvent<CustomerEventSource, CustomerCreated>(
                context => new CustomerEventSource(
                    context.Entity.Id.ToString(),
                    context.Entity.Name,
                    context.Entity.Email),
                options => options.UseExpectedVersion(ExpectedVersion.NoStream));
    }
}

public sealed record CustomerEventSource(string CustomerId, string Name, string Email);
```

Then map the source to the event payload with your mapper adapter:

```csharp
public sealed class CommerceEventMappingProfile : OctoMapProfile
{
    public override void Configure(IOctoMapConfigurationBuilder builder)
    {
        builder.CreateMap<CustomerEventSource, CustomerCreated>();
    }
}
```

`ToEvent<TEvent>()` can be repeated for the same command/entity pair. Use `When(...)` for conditional events and `UseExpectedVersion(...)` for optimistic concurrency rules.

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

`TurtlePath.Jobs` provides a standard execution path for two common workloads:

- one-shot jobs, usually used by console apps or Kubernetes CronJobs
- recurring background jobs, usually hosted inside a worker service

A job is a regular DI service. Put dependencies in the constructor and implement only the work in `ExecuteAsync`:

```csharp
public sealed class ImportCustomersJob : TurtlePathJob
{
    private readonly ICustomerImportService customerImportService;
    private readonly ILogger<ImportCustomersJob> logger;

    public ImportCustomersJob(
        ICustomerImportService customerImportService,
        ILogger<ImportCustomersJob> logger)
    {
        this.customerImportService = customerImportService;
        this.logger = logger;
    }

    public override async Task ExecuteAsync(TurtlePathJobContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Running {JobName}. Execution={ExecutionId}, Attempt={Attempt}",
            context.JobName,
            context.ExecutionId,
            context.Attempt);

        await customerImportService.ImportAsync(cancellationToken);
    }
}
```

### One-Shot Jobs

For Kubernetes CronJobs or console workloads, register one or more jobs with `AddJob<TJob>()` and run the manager from `Program.cs`.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TurtlePath.ExceptionHandling;
using TurtlePath.Jobs;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTurtlePathExceptionHandlingCore(options =>
{
    options.For<InvalidOperationException>(
        ExceptionKind.Business,
        exception => exception.Message);
});

builder.Services.AddScoped<ICustomerImportService, CustomerImportService>();
builder.Services.AddScoped<IInvoiceImportService, InvoiceImportService>();

builder.Services.AddTurtlePathJobs(options =>
{
    options.ExecutionMode = TurtlePathJobExecutionMode.Parallel;
    options.MaxDegreeOfParallelism = 4;
    options.Retries = 2;
    options.RetryDelay = TimeSpan.FromSeconds(10);
    options.FailureBehavior = TurtlePathJobFailureBehavior.Rethrow;
})
.AddJob<ImportCustomersJob>("import-customers")
.AddJob<ImportInvoicesJob>("import-invoices");

using var host = builder.Build();

var result = await host.Services.RunTurtlePathJobsAsync();

return result.Succeeded ? 0 : 1;
```

The manager waits for the whole batch to finish. In `Parallel` mode, registered jobs run at the same time up to `MaxDegreeOfParallelism`; in `Sequential` mode, they run one by one. You can also run a selected subset when a process receives the requested job names from command-line arguments:

```csharp
var selectedJobs = args switch
{
    [ "customers" ] => new[] { typeof(ImportCustomersJob) },
    [ "invoices" ] => new[] { typeof(ImportInvoicesJob) },
    _ => new[] { typeof(ImportCustomersJob), typeof(ImportInvoicesJob) }
};

var result = await host.Services.RunTurtlePathJobsAsync(selectedJobs);
```

If a job fails, TurtlePath retries it using the configured `Retries` and `RetryDelay`. After retries are exhausted:

- `Rethrow` throws a `TurtlePathJobManagerException`, which is usually right for Kubernetes because the pod exits as failed.
- `Continue` records the failed job result and keeps processing the rest of the batch.
- `StopHost` asks the host to stop when the job is running from a hosted service.

### Recurring Background Jobs

For long-running worker services, register recurring jobs with `AddCronJob<TJob>()`. TurtlePath adds a hosted service that manages independent loops for every registered cron job:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TurtlePath.Jobs;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddScoped<IRefreshCatalogService, RefreshCatalogService>();
builder.Services.AddScoped<ICustomerSyncService, CustomerSyncService>();

builder.Services.AddTurtlePathJobs()
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

await builder.Build().RunAsync();
```

`RunOnStart` runs the job once as soon as the worker starts, then continues using the configured interval. Each execution creates a fresh DI scope and runs through `IBackgroundExceptionBoundary`, so retries, reporting, and failure behavior are consistent with the rest of TurtlePath exception handling.

Use one-shot jobs when the process should finish after the work is done. Use recurring background jobs when the process should stay alive and execute the same work repeatedly.

## Analyzers

`TurtlePath.Analyzers` protects the main sharp edge of scalar `CId`: two entities can expose `CId` while masking different value types. For example, a clean `Customer` may use the default `Guid` configuration while a legacy `Invoice` stores its id as `int`.

```csharp
if (customer.Id == invoice.Id) // TP0001
{
}

customer.Id = invoice.Id; // TP0002
```

The analyzer uses the CId registrations it can see in source, such as `UseCId<Guid, string>()` and `UseCIdFor<LegacyInvoice, int, int>()`. It only reports when it can infer both entity id value types.


