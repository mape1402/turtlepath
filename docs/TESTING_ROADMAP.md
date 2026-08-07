# TurtlePath Testing Roadmap

## Purpose

Create a testing package that helps developers write unit and integration tests for TurtlePath applications without rebuilding the same dependency graph, mocks, fake storage, mapper setup, validator setup, hooks, and Pelican wiring in every test project.

The package must support the recommended TurtlePath path, but it must also be extensible enough to become the reference model for future testing packages across the Elysium ecosystem.

## Goals

- Provide a test harness for TurtlePath handlers.
- Provide an integration test host for TurtlePath automations.
- Avoid coupling tests to a specific mock framework.
- Allow developers to override only the behavior needed by each test.
- Support both fake in-memory testing and realistic SQLite-backed integration testing.
- Keep adapter-specific testing helpers isolated from the core testing package.
- Establish a reusable pattern for future packages such as OctoMap, Crabalidator, Pigeon, Spider, and other Elysium libraries.

## Non-Goals

- Do not generate source code as the first implementation path.
- Do not hide business assertions behind excessive testing magic.
- Do not force Moq, NSubstitute, FakeItEasy, or any specific mocking framework.
- Do not make `TurtlePath.Testing` depend on Entity Framework Core, OctoMap, Crabalidator, or other optional adapters.
- Do not replace existing unit test frameworks such as xUnit, NUnit, or MSTest.

## Package Strategy

### TurtlePath.Testing

Core testing package for handler and automation tests.

Responsibilities:

- Build a test service provider with TurtlePath defaults.
- Register fake implementations for TurtlePath abstractions.
- Provide a simple test host for resolving handlers and sending Pelican requests.
- Provide in-memory entity storage for happy-path tests.
- Provide delegate-based mapper and validator adapters.
- Provide hook tracing and hook registration helpers.
- Provide assertion-friendly access to stored entities and executed operations.

This package should be usable by any test project without bringing infrastructure dependencies.

### TurtlePath.Testing.EntityFrameworkCore

Integration testing helpers for EF-backed applications.

Responsibilities:

- Register SQLite-backed DbContexts.
- Create and reset schemas.
- Seed entities.
- Provide helpers for per-test database isolation.
- Reuse TurtlePath EF configuration and CId conversion behavior.

This package should be optional and installed only when tests need real EF behavior.

### Future Adapter Testing Packages

Adapter-specific packages should be created only when the adapter has enough testing friction to justify it.

Possible future packages:

- `TurtlePath.Testing.OctoMap`
- `TurtlePath.Testing.Crabalidator`
- `TurtlePath.Testing.EventSourcing`
- `Pigeon.Testing`
- `Spider.Testing`

The rule should be: keep the core package based on abstractions, and move adapter-specific setup into adapter-specific testing packages.

## Testing Levels

### 1. Handler Unit Tests

Used when the developer has a concrete handler class and wants to test the handler behavior directly.

Characteristics:

- No real database.
- No real external adapters.
- Fake storage.
- Delegate mapper.
- Delegate validator.
- Optional hook tracing.
- Can resolve the handler from DI or instantiate through the test host.

Example target usage:

```csharp
await using var host = await TurtlePathTestHost
    .Create()
    .UseInMemoryStorage()
    .WithMap<CreateCustomerRequest, Customer>(request => new Customer(request.Name, request.Email))
    .WithMap<Customer, CustomerResponse>(customer => new CustomerResponse(customer.Id, customer.Name, customer.Email))
    .WithValidRequest<CreateCustomerRequest>()
    .BuildAsync();

var handler = host.Resolve<CreateCustomerCommandHandler>();

var response = await handler.Handle(request, CancellationToken.None);

host.Store<Customer>().ShouldContain(customer => customer.Email == request.Email);
```

### 2. Handler Integration Tests

Used when the developer wants to test a handler through the same dispatch path used by the application.

Characteristics:

- Real DI.
- Real Pelican dispatch.
- Real TurtlePath registration.
- Fake or SQLite persistence.
- Optional real adapters.

Example target usage:

```csharp
await using var host = await TurtlePathTestHost
    .Create()
    .UseTurtlePath()
    .UseInMemoryStorage()
    .WithMap<CreateCustomerRequest, Customer>(request => new Customer(request.Name, request.Email))
    .WithMap<Customer, CustomerResponse>(customer => new CustomerResponse(customer.Id, customer.Name, customer.Email))
    .BuildAsync();

var response = await host.SendAsync<CreateCustomerResponse>(request);
```

### 3. Automation Integration Tests

Used when the flow is created through TurtlePath Automations and no explicit handler exists in the application code.

Characteristics:

- Must use DI.
- Must use Pelican dispatch.
- Must register automation profiles.
- Validates generated handlers indirectly through the public request path.
- Can run with in-memory storage or SQLite.

Example target usage:

```csharp
await using var host = await TurtlePathTestHost
    .Create()
    .UseTurtlePath()
    .UseAutomations(typeof(CustomerAutomationProfile).Assembly)
    .UseInMemoryStorage()
    .BuildAsync();

var response = await host.SendAsync<CreateCustomerResponse>(request);

host.Store<Customer>().ShouldContain(customer => customer.Id == response.Id);
```

### 4. Adapter Integration Tests

Used when the behavior being tested depends on a real adapter.

Examples:

- OctoMap mapping profiles.
- Crabalidator validators.
- EF Core persistence.
- Event sourcing append behavior.
- Pigeon message publishing.
- Spider boundaries.

Adapter tests should use adapter-specific packages when available.

## Public API Design

### TurtlePathTestHost

Main runtime object used by tests.

Expected capabilities:

- `SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)`
- `SendAsync(IRequest request, CancellationToken cancellationToken = default)`
- `Resolve<TService>()`
- `Store<TEntity>()`
- `Services`
- `DisposeAsync()`

### TurtlePathTestHostBuilder

Main fluent builder used to configure the test host.

Expected capabilities:

- `UseTurtlePath(...)`
- `UseAutomations(...)`
- `UseInMemoryStorage()`
- `ConfigureServices(...)`
- `WithService<TService>(...)`
- `WithSingleton<TService>(...)`
- `WithScoped<TService>(...)`
- `WithTransient<TService>(...)`
- `WithMap<TSource, TDestination>(Func<TSource, TDestination> map)`
- `WithValidator<TRequest>(Func<TRequest, ValidationResult> validate)`
- `WithValidRequest<TRequest>()`
- `WithInvalidRequest<TRequest>(...)`
- `WithSeed<TEntity>(...)`
- `TraceHooks()`
- `BuildAsync()`

### InMemoryEntityStore

Testing storage abstraction for entity scenarios.

Expected capabilities:

- Add entities.
- Update entities.
- Delete entities.
- Query by id.
- Query all.
- Clear data.
- Inspect operations.

The storage must respect the TurtlePath abstractions instead of coupling directly to EF.

### DelegateMapperAdapter

Testing mapper based on explicit delegates.

Expected behavior:

- Developer registers source and destination mappings.
- Missing mappings fail with a clear error message.
- No reflection-based automatic mapping in the core package.

### DelegateValidatorAdapter

Testing validator based on explicit delegates.

Expected behavior:

- Developer can mark a request as valid.
- Developer can return validation failures.
- Missing validators can default to valid only when explicitly configured by the builder.

### HookTrace

Testing utility that records executed hooks.

Expected capabilities:

- Record hook type.
- Record request type.
- Record entity type.
- Record execution order.
- Allow assertions over hook execution.

## Entity Framework Core Testing Package

### TurtlePath.Testing.EntityFrameworkCore

Expected capabilities:

- `UseSqliteDbContext<TDbContext>(...)`
- `UseSqliteInMemoryDatabase()`
- `CreateSchemaAsync()`
- `ResetDatabaseAsync()`
- `SeedAsync<TEntity>(...)`
- `WithDbContext<TDbContext>()`

The package must validate real EF behavior, including:

- CId conversion.
- Entity configuration.
- DbContext base behavior.
- Queries.
- Saves.
- Transactions where applicable.

## CId Testing Support

The testing package should make CId scenarios simple but explicit.

Required scenarios:

- All entities use the same CId backing type.
- Different entities use different CId backing types.
- CId backed by `Guid`.
- CId backed by `int`.
- CId backed by `Ulid` and stored as `string`.

Testing helpers should avoid static metadata and must use the same configured CId services used by the runtime packages.

## Automations Testing Support

The package must support testing automations as integration tests.

Required scenarios:

- Create automation.
- Update automation.
- Delete automation.
- Patch automation.
- Query by id automation.
- Query paged automation.
- Multiple automations for the same entity with different request/response models.
- Custom entity contracts that do not use the recommended `BaseEntity` path.

The test host should make it easy to register automation profiles from assemblies.

## Exception Handling Testing Support

The package should support the current TurtlePath exception handling model.

Required scenarios:

- Register exception profiles.
- Resolve exception handling for custom kinds.
- Assert the selected exception rule.
- Assert the generated HTTP response through the HTTP-specific package or API test host.
- Assert non-HTTP exception handling behavior without referencing HTTP abstractions.

## Jobs Testing Support

The package should support TurtlePath jobs.

Required scenarios:

- Execute one-shot jobs.
- Execute multiple one-shot jobs through the job manager.
- Assert job success.
- Assert job failure handling.
- Assert exception boundary usage.
- Test background job execution without waiting for real time delays.
- Control retries and fallback behavior deterministically.

The testing package should include a fake clock or scheduler abstraction only if the runtime job package exposes one. Otherwise, this should be deferred until the runtime API supports deterministic scheduling.

## Event Sourcing Testing Support

Event sourcing testing helpers should likely live in a dedicated package because they depend on event store behavior.

Possible package:

- `TurtlePath.Testing.EventSourcing`

Expected capabilities:

- Assert appended events.
- Assert stream names.
- Assert expected versions.
- Assert event order.
- Seed event streams.
- Use in-memory event store.

This package should be implemented after the core testing package is stable.

## Elysium Ecosystem Pattern

TurtlePath should establish the pattern, but not prematurely create a generic shared package.

Recommended approach:

1. Build `TurtlePath.Testing`.
2. Build `TurtlePath.Testing.EntityFrameworkCore`.
3. Let one or two other libraries create their own testing packages.
4. Extract shared pieces into `Elysium.Testing` only after duplication is real.

Potential shared concepts:

- Test host lifecycle.
- Service registration helpers.
- Fake clock.
- Operation trace.
- Scenario runner.
- Test data builders.
- Adapter replacement helpers.

## Implementation Phases

### Phase 1 - Package Skeleton

- Add `src/TurtlePath.Testing`.
- Add project metadata for NuGet publishing.
- Add package icon, README packaging metadata, license, repository metadata, and SourceLink settings through existing repo conventions.
- Add the project to the solution.
- Add `tests/TurtlePath.Testing.Tests`.
- Ensure test and sample projects are not packable.

Deliverable:

- Empty package compiling and packable.

### Phase 2 - Core Test Host

- Implement `TurtlePathTestHost`.
- Implement `TurtlePathTestHostBuilder`.
- Support custom `IServiceCollection` configuration.
- Support service resolution.
- Support async disposal.
- Support Pelican send helpers.

Deliverable:

- Tests can create a host, register services, resolve services, and send requests.

### Phase 3 - Delegate Adapters

- Implement delegate mapper adapter.
- Implement delegate validator adapter.
- Add builder methods for common mapper and validator setup.
- Ensure missing mappings fail with actionable errors.
- Ensure validation failures behave like runtime validation failures.

Deliverable:

- Handler tests can run without OctoMap or Crabalidator.

### Phase 4 - In-Memory Storage

- Implement in-memory storage adapter for TurtlePath storage abstractions.
- Support add, update, delete, query by id, and paged query scenarios.
- Add inspection helpers for stored entities.
- Add operation tracing.

Deliverable:

- Create, update, delete, query by id, and paged query handlers can be tested without a database.

### Phase 5 - Hook Testing

- Add hook tracing.
- Add test helpers for registering before/after hooks.
- Add assertions or query APIs over hook execution order.
- Validate compatibility with virtual handler methods and hook services.

Deliverable:

- Tests can prove hooks executed in the expected order with expected request/entity context.

### Phase 6 - Handler Testing Scenarios

- Add tests for generic handlers.
- Add tests for `BaseEntity` handlers.
- Add tests for handlers with response.
- Add tests for handlers without response.
- Add tests for custom entity contracts using `IEntity<TKey>`.

Deliverable:

- Core manual handler test cases are covered.

### Phase 7 - Automation Testing Scenarios

- Add automation test host helpers.
- Register automation profiles from assemblies.
- Test create, update, delete, patch, query by id, and paged query automations.
- Test multiple automations for the same entity.
- Test custom entity contracts.

Deliverable:

- Automations can be tested through Pelican without manually resolving generated handlers.

### Phase 8 - Entity Framework Core Testing Package

- Add `src/TurtlePath.Testing.EntityFrameworkCore`.
- Add SQLite integration helpers.
- Add schema creation/reset helpers.
- Add seed helpers.
- Add tests for CId conversion and EF persistence.

Deliverable:

- Consumers can write realistic integration tests with SQLite using a small amount of setup.

### Phase 9 - Documentation and Samples

- Update root README with testing package usage.
- Add detailed testing guide under `docs`.
- Add sample tests to the sample project or a dedicated sample test project.
- Document when to use unit tests vs integration tests.
- Document how adapter-specific packages should be approached.

Deliverable:

- Developers can copy a working example for handlers, automations, and EF SQLite tests.

### Phase 10 - Future Extraction Review

- Review whether shared testing infrastructure should move to `Elysium.Testing`.
- Identify duplicated patterns from other Elysium libraries.
- Decide whether generator-based test scaffolding is still needed.

Deliverable:

- A grounded decision based on actual usage instead of speculative abstraction.

## Recommended First Milestone

The first release should include:

- `TurtlePath.Testing`.
- Handler unit testing with fake storage.
- Automation integration testing with fake storage.
- Delegate mapper and validator adapters.
- Hook trace support.
- Documentation and examples.

The EF SQLite package should be the second milestone unless a current migration requires it immediately.

