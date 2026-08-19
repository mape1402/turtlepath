# TurtlePath

TurtlePath is a reusable .NET library for building Pelican-based application handlers, automations, jobs, exception boundaries, templates, and testing helpers.

It packages the template's base command/query handlers, handler hook pipeline, validation and mapping adapters, storage abstractions, response/request primitives, custom identifier support, and Entity Framework helper configuration into a standalone library.

## Packages

The recommended package set is:

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
- `TurtlePath.Spider`: Spider extension methods for dispatching Pelican requests through Spider boundaries without coupling Spider and Pelican to each other.
- `TurtlePath.Spider.Transactions`: ambient transaction boundary implementation, request filtering, profile discovery, and chainable Spider registration without an EF Core dependency.
- `TurtlePath.Testing`: test host, delegate adapters, and in-memory storage for TurtlePath handler and automation tests.
- `TurtlePath.Testing.EntityFrameworkCore`: SQLite-backed integration testing helpers for EF Core TurtlePath applications.
- `TurtlePath.Testing.EventSourcing`: event stream assertion helpers for TurtlePath event sourcing tests.
- `TurtlePath.Testing.Integration`: thin test-host wrappers for integration testing adapters such as Pelican, OctoMap, Crabalidator, Pigeon, Spider, DynaBee, Krackend event sourcing, and DataScorpio.
- `TurtlePath.Template`: official `dotnet new` template for creating TurtlePath services.
- `TurtlePath.OctoMap`: mapper adapter for the OctoMap mapping stack.
- `TurtlePath.Crabalidator`: validator adapter for the Crabalidator validation stack.
- `TurtlePath.DataScorpio`: recommended DataScorpio filtering and sorting adapter for query criteria.
- `TurtlePath.Sieve`: optional Sieve filtering and sorting adapter kept for projects that still use Sieve.
- `TurtlePath.Analyzers`: optional compile-time checks for unsafe `CId` usage across entities with different configured identifier value types.
- `TurtlePath.Template.HeroesShowcase`: official `dotnet new` demo template that generates a complete Heroes service showcasing TurtlePath features end to end.
- `TurtlePath.Studio.Tool`: .NET tool that installs and updates TurtlePath Studio from GitHub Releases.

Alternative adapters are available when a project needs them: `TurtlePath.AutoMapper` and `TurtlePath.FluentValidation`.

## Install

```powershell
dotnet add package TurtlePath
```

Install the service template when you want to create a new TurtlePath project:

```powershell
dotnet new install TurtlePath.Template
```

Create the default API/consumer host:

```powershell
dotnet new turtlepath -n MyService --host api-consumer
```

Create a one-shot job host for console or Kubernetes CronJob execution:

```powershell
dotnet new turtlepath -n MyJob --host job
```

Install the Heroes Showcase demo template when you want a full reference project:

```powershell
dotnet new install TurtlePath.Template.HeroesShowcase
dotnet new turtlepath-heroes-showcase -n Heroes.Service
```

Install TurtlePath Studio when you prefer creating projects from the desktop UI:

```powershell
dotnet tool install TurtlePath.Studio.Tool --global
turtlepath-studio install
```

The installer downloads the latest published Studio release, installs it under `%LOCALAPPDATA%\TurtlePath\Studio`, and creates a desktop shortcut named `TurtlePath Studio`.

Update Studio later with:

```powershell
turtlepath-studio update
```

Studio can also update itself from `Environment > Studio updates`. The Studio and its updater are distributed as the `TurtlePath.Studio` NuGet package. Studio-specific release notes live in [`STUDIO_CHANGELOG.md`](STUDIO_CHANGELOG.md).

Useful options:

```powershell
turtlepath-studio install --launch
turtlepath-studio install --output C:\Tools\TurtlePathStudio
turtlepath-studio install --no-shortcut
turtlepath-studio update --version studio-v1.0.0
```

## Template Defaults

Generated API/consumer services start with Scalar OpenAPI UI, Spider pipeline boundaries, TurtlePath exception handling, DataScorpio filtering, OctoMap mapping, Crabalidator validation, EF Core storage adapters, jobs, and testing foundations.

Pigeon messaging 2.4.0 and EventSourcing are included as opt-in template surfaces. Pigeon 2.4.0 adds configurable consumer throughput through `MaxConcurrency` and `QueueCapacity`; they stay disabled by default so a new service can start without broker or event-store settings. Enable them from the API dependency registration when the service actually needs messaging or append-only event history.

The generated project keeps layer ownership explicit: Business references Domain and TurtlePath abstractions, API owns host composition, and Persistence owns the concrete EF Core `DbContext`. Business code that needs persistence should depend on `IDbContext`, not the concrete `AppDbContext`.

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
dotnet add package TurtlePath.Spider
dotnet add package TurtlePath.Spider.Transactions
dotnet add package TurtlePath.Testing
dotnet add package TurtlePath.Testing.EntityFrameworkCore
dotnet add package TurtlePath.Testing.EventSourcing
dotnet add package TurtlePath.Testing.Integration
dotnet add package TurtlePath.DataScorpio
dotnet add package TurtlePath.OctoMap
dotnet add package TurtlePath.Crabalidator
dotnet add package TurtlePath.Analyzers
```

Use one mapper adapter package and one validation adapter package. The recommended pair is `TurtlePath.OctoMap` and `TurtlePath.Crabalidator`.

Analyzer packages should stay private to the project that consumes them:

```xml
<PackageReference Include="TurtlePath.Analyzers" Version="..." PrivateAssets="all" />
```

Testing packages should normally stay in test projects:

```xml
<PackageReference Include="TurtlePath.Testing" Version="..." PrivateAssets="all" />
<PackageReference Include="TurtlePath.Testing.EntityFrameworkCore" Version="..." PrivateAssets="all" />
<PackageReference Include="TurtlePath.Testing.EventSourcing" Version="..." PrivateAssets="all" />
<PackageReference Include="TurtlePath.Testing.Integration" Version="..." PrivateAssets="all" />
```

`TurtlePath.Template` and `TurtlePath.Template.HeroesShowcase` are installed through `dotnet new install` instead of referenced from an application project.

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
    .UseDataScorpio(profiles => profiles.FromAssemblyOf<MyApplicationMarker>())
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

Register the Spider transaction boundary with the application assemblies that contain requests and transaction profiles. Keep this registration in the host composition root:

```csharp
services.AddTurtlePathSpiderTransactions(
    configuration,
    typeof(MyApplicationMarker).Assembly,
    typeof(Program).Assembly);
```

The registration does not scan the entire `AppDomain`; test hosts and unrelated dependencies are never included. Feature-specific transaction profiles can remain in the Business assembly and are discovered automatically from the assemblies supplied here.

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

When a mutation response exposes fields derived from navigation properties, configure the mutation to read the entity again before mapping the response:

```csharp
builder.For<Customer>()
    .ToCreate<CreateCustomerRequest, CustomerResponse>(mutation => mutation
        .Include(customer => customer.AccountManager))
    .ToUpdate<UpdateCustomerRequest, CustomerResponse>(mutation => mutation
        .ReloadBeforeResponse()
        .Include(customer => customer.AccountManager));
```

`Include(...)` implies response projection from storage. Use `ReloadBeforeResponse()` when the response should be rebuilt from storage even without navigation includes.

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

TurtlePath testing packages give test projects the same composition shape as the application without forcing Moq, NSubstitute, or any other mock framework. The full guide lives in `docs/TESTING_GUIDE.md`, and the runnable examples live in `samples/TurtlePath.Samples.Testing`.

Use `TurtlePath.Testing` for fast handler unit tests and lightweight integration tests:

- `TurtlePathTestHost.Create()` starts the fluent test host builder.
- `WithMap<TSource, TDestination>(...)` configures mapper behavior with delegates.
- `WithUpdateMap<TSource, TDestination>(...)` configures update/patch-style mapping.
- `WithValidRequest<TRequest>()` and `WithValidator<TRequest>(...)` configure validation.
- `WithSeed(...)` seeds the in-memory storage.
- `UsePelican(...)` dispatches requests through Pelican.
- `UseAutomations(...)` registers automation profiles and generated handlers.
- `TraceHooks()` records hook stage execution through `HookTrace`.
- `Store<TEntity>()` exposes assertion-friendly in-memory entities.
- `Storage.Operations` records add, update, remove, and save operations.

Direct handler unit test:

```csharp
await using var host = await TurtlePathTestHost
    .Create()
    .WithMap<CreateCustomerRequest, Customer>(request => new Customer
    {
        Id = request.Id,
        Name = request.Name
    })
    .WithMap<Customer, CustomerResponse>(customer => new CustomerResponse
    {
        Id = customer.Id,
        Name = customer.Name
    })
    .WithValidRequest<CreateCustomerRequest>()
    .TraceHooks()
    .BuildAsync();

var handler = new CreateCustomerCommandHandler(host.Services);

var response = await handler.Handle(new CreateCustomerRequest(1, "Ada"));

Assert.Equal("Ada", response.Name);
Assert.True(host.Store<Customer>().Contains(customer => customer.Id == 1));
Assert.Contains(host.Storage.Operations, operation => operation.Action == "SaveChanges");
Assert.Contains(host.Resolve<HookTrace>().Entries, entry => entry.Stage == "AfterSave");
```

`TurtlePath.Sieve` remains available for services that still depend on Sieve attributes or Sieve query configuration. New services should prefer `TurtlePath.DataScorpio`.

Pelican integration test for a manual handler:

```csharp
await using var host = await TurtlePathTestHost
    .Create()
    .UsePelican(typeof(CreateCustomerCommandHandler).Assembly)
    .WithMap<CreateCustomerRequest, Customer>(request => new Customer { Id = request.Id, Name = request.Name })
    .WithMap<Customer, CustomerResponse>(customer => new CustomerResponse { Id = customer.Id, Name = customer.Name })
    .BuildAsync();

var response = await host.SendAsync(new CreateCustomerRequest(1, "Ada"));
```

Automation integration test:

```csharp
await using var host = await TurtlePathTestHost
    .Create()
    .UseAutomations(typeof(CustomerAutomationProfile).Assembly)
    .WithMap<CreateCustomerRequest, Customer>(request => new Customer { Id = request.Id, Name = request.Name })
    .WithMap<Customer, CustomerResponse>(customer => new CustomerResponse { Id = customer.Id, Name = customer.Name })
    .BuildAsync();

var response = await host.SendAsync(new CreateCustomerRequest(1, "Ada"));
```

Use `TurtlePath.Testing.EntityFrameworkCore` when the test must prove real EF Core behavior with SQLite:

```csharp
await using var host = await TurtlePathTestHost
    .Create()
    .UseAutomations(typeof(CustomerAutomationProfile).Assembly)
    .UseSqliteDbContext<AppDbContext>(options => options with
    {
        ConfigurationAssemblies = [typeof(AppDbContext).Assembly]
    })
    .WithMap<CreateCustomerRequest, Customer>(request => new Customer { Id = request.Id, Name = request.Name })
    .WithMap<Customer, CustomerResponse>(customer => new CustomerResponse { Id = customer.Id, Name = customer.Name })
    .BuildAsync();

await host.CreateSchemaAsync<AppDbContext>();

var response = await host.SendAsync(new CreateCustomerRequest(1, "Ada"));
```

Use `TurtlePath.Testing.EventSourcing` when a command should append events:

```csharp
var events = await host.ReadEventStreamAsync("customers", customerId);

Assert.Contains(events, item => item.EventType == "customer-created");
```

The test host also supports `UseExceptionHandling()`, `HandleException(...)`, `UseJobs()`, `WithJob<TJob>()`, and `RunJobsAsync()` for TurtlePath exception and job scenarios.

Use `TurtlePath.Testing.Integration` when a test should compose TurtlePath with the real testing adapters owned by the surrounding libraries:

```csharp
await using var host = await TurtlePathTestHost
    .Create()
    .UsePelicanTesting(typeof(CreateCustomerCommandHandler).Assembly)
    .UseOctoMapTesting(typeof(CustomerMappingProfile).Assembly)
    .UseCrabalidatorTesting(typeof(CreateCustomerRequestValidator).Assembly)
    .UsePigeonTesting(typeof(CustomersHubConsumer).Assembly)
    .UseSpiderTesting(typeof(TransactionBoundary).Assembly)
    .UseDynaBeeTesting()
    .UseKrackendTesting()
    .UseDataScorpioTesting(profiles => profiles.FromAssemblyOf<CustomerQueryProfile>())
    .BuildAsync();
```

When the test should start from the same dependency registration as the application, use `CreateFromServices(...)` and layer testing helpers on top:

```csharp
await using var host = await TurtlePathTestHost
    .CreateFromServices(services => services.AddDefaults(configuration, environment))
    .UseOctoMapTesting(typeof(CustomerMappingProfile).Assembly)
    .UseCrabalidatorTesting(typeof(CreateCustomerRequestValidator).Assembly)
    .UseDataScorpioTesting(profiles => profiles.FromAssemblyOf<CustomerQueryProfile>())
    .BuildAsync();
```

These methods are intentionally thin wrappers. OctoMap owns mapping assertions, Crabalidator owns validator assertions, Pelican owns dispatch tracing, Pigeon owns message transport assertions, Spider owns boundary tracing, DynaBee owns generated assembly assertions, Krackend owns event store assertions, and DataScorpio owns filter/sort/paging assertions.

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

Use profiles when the exception catalog starts growing. Core profiles describe the exception once in a transport-neutral way:

```csharp
public static class CommerceExceptionKinds
{
    public static readonly ExceptionKind SubscriptionExpired = new("subscription_expired");
}

public sealed class SubscriptionExpiredException : Exception
{
    public SubscriptionExpiredException(string customerId)
        : base($"Customer '{customerId}' has an expired subscription.")
    {
        CustomerId = customerId;
    }

    public string CustomerId { get; }
}

public sealed class CommerceExceptionProfile : ExceptionHandlingProfile
{
    public override void Configure(ExceptionHandlingOptionsBuilder builder)
    {
        builder.For<SubscriptionExpiredException>(
            CommerceExceptionKinds.SubscriptionExpired,
            exception => $"Subscription expired for customer '{exception.CustomerId}'.");
    }
}
```

Target profiles decide how each host type reacts to the descriptor:

```csharp
public sealed class CommerceHttpExceptionProfile : HttpExceptionHandlingProfile
{
    public override void Configure(HttpExceptionHandlingOptionsBuilder builder)
    {
        builder.Map(CommerceExceptionKinds.SubscriptionExpired, StatusCodes.Status403Forbidden);
    }
}

public sealed class CommerceConsumerExceptionProfile : ConsumerExceptionHandlingProfile
{
    public override void Configure(ConsumerExceptionHandlingOptionsBuilder builder)
    {
        builder.RethrowWhen((descriptor, _) =>
            descriptor.Kind != CommerceExceptionKinds.SubscriptionExpired);
    }
}

public sealed class CommerceBackgroundExceptionProfile : BackgroundExceptionHandlingProfile
{
    public override void Configure(BackgroundExceptionHandlingOptionsBuilder builder)
    {
        builder.RethrowWhen(descriptor =>
            descriptor.Kind == ExceptionKind.Transient);
    }
}
```

Applications can register profiles explicitly:

```csharp
services.AddExceptionHandlingProfiles(typeof(CommerceExceptionProfile).Assembly);
services.AddHttpExceptionHandlingProfiles(typeof(CommerceHttpExceptionProfile).Assembly);
services.AddConsumerExceptionHandlingProfiles(typeof(CommerceConsumerExceptionProfile).Assembly);
services.AddBackgroundExceptionHandlingProfiles(typeof(CommerceBackgroundExceptionProfile).Assembly);
```

Projects generated from `TurtlePath.Template` discover exception handling profiles automatically from the Business and API assemblies. Put service-specific profiles next to the feature that owns the exception, and do not call the TurtlePath exception adapter registrations again from the custom container.

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

## Documentation

Public documentation lives under `docs`:

- `docs/ARCHITECTURE.md` covers the package boundaries and design intent.
- `docs/TESTING_GUIDE.md` covers unit and integration testing helpers.
- `docs/RELEASE.md` covers the release checklist.

## Release Tracks

TurtlePath has separate release tracks so package notes do not get mixed:

- Runtime libraries use `.release`, `CHANGELOG.md`, and `build-and-release.yml`.
- The service template uses `.template.release`, `TEMPLATE_CHANGELOG.md`, and `template-release.yml`.
- Versioned documentation uses `.docs.release`, `DOCS_CHANGELOG.md`, and `docs-release.yml`.
- TurtlePath Studio uses `.studio.release`, `STUDIO_CHANGELOG.md`, and `studio-release.yml`.


