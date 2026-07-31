# TurtlePath.Automations Roadmap

## Goal

Build `TurtlePath.Automations` as the fast path for standard application flows.
Consumers should be able to create DTOs, entities, mappings, validators, and a small automation profile, then resolve the usual happy paths through Pelican without writing handlers by hand.

The package must generate or register concrete Pelican handlers from declarative metadata while preserving TurtlePath's current extension model:

- Mapping through `IMapperAdapter`.
- Validation through `IValidatorAdapter`.
- Persistence through `IStorageReaderAdapter` and `IStorageWriterAdapter`.
- Query criteria through storage criteria adapters.
- Behavior extension through TurtlePath hooks.
- Transversal happy-path customization through flow executors and steps.
- Manual handlers for cases that need full control.

## Principles

- Automations are a convenience layer, not a replacement for handlers.
- DI registration discovers and registers automation profiles; flow configuration lives in profiles.
- Attributes are supported for simple local declarations, but profiles are the recommended API.
- Profile configuration wins over attributes when both exist.
- Conflicting registrations fail during startup with clear errors.
- Generated handlers must be normal Pelican handlers so consumers continue using `mediator.Send(...)`.
- Existing handler virtual methods must remain as the local override escape hatch for backwards compatibility.
- Default virtual method implementations should delegate to flow steps where possible.
- The package must not depend on EF, OctoMap, Crabalidator, AutoMapper, FluentValidation, or Sieve directly.
- Default entity support targets `BaseEntity` and `CId`.
- Custom entity support targets any `IEntity<TKey>`.

## Public API Shape

Recommended profile usage:

```csharp
public sealed class SalesAutomationProfile : TurtlePathAutomationProfile
{
    public override void Configure(ITurtlePathAutomationBuilder builder)
    {
        builder.For<Customer>()
            .ToCreate<CreateCustomerCommand, CustomerResponse>()
            .ToUpdate<UpdateCustomerCommand, CustomerResponse>()
            .ToDelete<DeleteCustomerCommand>()
            .ToPatch<PatchCustomerCommand>()
            .ToGetById<GetCustomerByIdQuery, CustomerResponse>()
            .ToGetPaged<SearchCustomersQuery, CustomerResponse>();

        builder.For<LegacyCustomer, int>()
            .ToUpdate<UpdateLegacyCustomerCommand, LegacyCustomerResponse>(operation =>
            {
                operation.GetKeyFrom(command => command.LegacyId);
            });
    }
}
```

DI usage:

```csharp
services.AddTurtlePath()
    .UseAutomations(typeof(SalesAutomationProfile).Assembly);
```

Attribute usage for simple cases:

```csharp
[CreateAutomation(typeof(Customer), typeof(CustomerResponse))]
public sealed class CreateCustomerCommand : IRequest<CustomerResponse>
{
}
```

## Phase 1 - Project Scaffolding

- Add `src/TurtlePath.Automations`.
- Add the project to `TurtlePath.sln`.
- Configure NuGet metadata through the existing repository conventions.
- Reference only the required TurtlePath packages and Dynabee.
- Add `tests/TurtlePath.Automations.Tests`.
- Add package icon/readme metadata alignment.

Exit criteria:

- Solution restores.
- New project builds without automation behavior implemented.
- Test project is included and can run.

## Phase 2 - Descriptor Model

- Introduce `AutomationDescriptor` as the internal normalized representation of an automation.
- Add operation kinds:
  - Create
  - Update
  - Delete
  - Patch
  - GetById
  - GetOne
  - GetMany
  - GetPaged
- Capture request type, response type, entity type, key type, return mode, and source.
- Capture operation customization:
  - Key selector.
  - Not-found behavior.
  - Default sort for paged queries.
  - Criteria source.
  - Projection hints where adapter support exists.
- Add descriptor validation and conflict detection.

Exit criteria:

- Profiles and attributes can both produce descriptors.
- Duplicate/conflicting descriptors are detected before handler registration.
- Tests cover descriptor creation and conflicts.

## Phase 3 - Flow Executors and Steps

- Extract the current handler happy paths into reusable executors and steps.
- Add command executors:
  - Create with response.
  - Create without response.
  - Update with response.
  - Update without response.
  - Delete with response.
  - Delete without response.
  - Patch with response.
  - Patch without response.
- Add query executors:
  - GetById.
  - GetOne.
  - GetMany.
  - GetPaged.
- Add default steps for the stable operation stages:
  - Validation.
  - Entity creation.
  - Entity lookup.
  - Entity mapping.
  - Patch application.
  - Save.
  - Delete.
  - Response mapping.
  - Query execution.
  - Criteria application.
- Keep hooks around the same semantic stages they currently cover.
- Keep existing handler virtual methods for backwards compatibility.
- Make default virtual method implementations delegate to the relevant step when possible.
- Ensure local handler overrides take precedence over step behavior for that specific handler.
- Allow selected steps to be replaced through DI for transversal behavior changes.

Expected behavior priority:

1. Manual handler override.
2. DI-replaced flow step.
3. TurtlePath default step.
4. Hooks around the flow stage.

Exit criteria:

- Existing manual handlers continue to compile.
- Existing overrides continue to execute.
- Default handler behavior remains equivalent to the current implementation.
- Tests cover virtual override compatibility.
- Tests cover replacing at least one transversal step through DI.

## Phase 4 - Profile API

- Add `TurtlePathAutomationProfile`.
- Add `ITurtlePathAutomationBuilder`.
- Add entity builder APIs:
  - `For<TEntity>()` for `BaseEntity` with `CId`.
  - `For<TEntity, TKey>()` for custom `IEntity<TKey>` entities.
- Add command APIs:
  - `ToCreate<TCommand, TResponse>()`
  - `ToCreate<TCommand>()`
  - `ToUpdate<TCommand, TResponse>()`
  - `ToUpdate<TCommand>()`
  - `ToDelete<TCommand, TResponse>()`
  - `ToDelete<TCommand>()`
  - `ToPatch<TCommand, TResponse>()`
  - `ToPatch<TCommand>()`
- Add query APIs:
  - `ToGetById<TQuery, TResponse>()`
  - `ToGetOne<TQuery, TResponse>()`
  - `ToGetMany<TQuery, TResponse>()`
  - `ToGetPaged<TQuery, TResponse>()`
- Add operation builders for customizations.

Exit criteria:

- Consumers can declare the target API in a profile.
- Fluent methods support repeated operations for the same entity with different request/response models.
- Tests cover fluent API descriptor output.

## Phase 5 - Attribute API

- Add simple automation attributes:
  - `CreateAutomationAttribute`
  - `UpdateAutomationAttribute`
  - `DeleteAutomationAttribute`
  - `PatchAutomationAttribute`
  - `GetByIdAutomationAttribute`
  - `GetOneAutomationAttribute`
  - `GetManyAutomationAttribute`
  - `GetPagedAutomationAttribute`
- Add assembly scanning for attributed request types.
- Keep attributes intentionally small; advanced configuration stays in profiles.
- Merge attribute descriptors with profile descriptors.
- Enforce profile-over-attribute precedence.

Exit criteria:

- Happy-path commands and queries can be automated using attributes only.
- Profile override behavior is deterministic and tested.
- Conflict errors are readable.

## Phase 6 - Handler Generation

- Use Dynabee to generate concrete Pelican handlers from descriptors.
- Generate handlers that extend existing TurtlePath generic handlers or call the new flow executors.
- Register generated types as Pelican request handlers in DI.
- Support handlers with response and without response.
- Ensure generated handlers use constructors compatible with DI.
- Avoid runtime request dispatchers or centralized domain managers.
- Keep generated handlers thin; operation behavior should live in executors and steps.

Exit criteria:

- `mediator.Send(command)` resolves generated handlers.
- Generated handlers run through existing TurtlePath validation, mapping, storage, and hooks.
- Tests cover create, update, delete, patch, and query handler resolution.

## Phase 7 - Patch Support

- Add `IPatchAction<TEntity>` for request-owned patch logic.
- Evaluate whether a dependency-aware patch contract is needed.
- Prefer hooks for collaboration-heavy patch flows.
- Validate patch automations at startup when the request does not implement the required contract.

Exit criteria:

- Patch automation works without writing a handler.
- Invalid patch requests fail during startup.
- Hooks still run around patch execution.

## Phase 8 - Query Customizations

- Support key selection for `GetById`.
- Support one/many query customization where current generic handlers allow it.
- Support paged query customization:
  - Criteria source.
  - Default sort.
  - Optional query adapter hints.
- Avoid binding the package to Sieve directly.

Exit criteria:

- Query happy paths work through generated handlers.
- Paged queries still use TurtlePath storage criteria abstractions.
- Customizations are covered by tests.

## Phase 9 - Sample Expansion

- Update the sample to show profile-based automations.
- Include:
  - Recommended `BaseEntity` + `CId` flow.
  - Custom `IEntity<int>` or `IEntity<Guid>` flow.
  - Create/update/delete with and without response.
  - Patch with `IPatchAction`.
  - GetById and paged query.
  - Hooks extending an automated flow.
  - Manual handler overriding a special case.
- Keep the sample using Pelican mediator, EF SQLite, OctoMap, and Crabalidator where possible.

Exit criteria:

- Sample demonstrates the intended developer experience.
- Sample compiles and runs.

## Phase 10 - Documentation

- Update `README.md` with automations as the recommended happy path.
- Prioritize Elysium adapters:
  - OctoMap.
  - Crabalidator.
- Document AutoMapper, FluentValidation, Sieve, and EF as optional adapters.
- Document when to use:
  - Attributes.
  - Profiles.
  - Hooks.
  - Manual handlers.
- Add package README details for `TurtlePath.Automations`.

Exit criteria:

- README reflects the new recommended flow.
- Package docs explain install, registration, profile setup, and extension points.

## Phase 11 - Hardening

- Add tests for startup validation.
- Add tests for duplicate operation registration.
- Add tests for generated handler DI registration.
- Add tests for profile and attribute precedence.
- Add tests for custom key entities.
- Verify package build.
- Update changelog.

Exit criteria:

- Full solution builds.
- Tests pass.
- NuGet packages can be produced except samples and tests.
- Changelog includes `TurtlePath.Automations`.
