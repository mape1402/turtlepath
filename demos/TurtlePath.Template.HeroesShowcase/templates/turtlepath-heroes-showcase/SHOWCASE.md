# Heroes Showcase

This project is a complete TurtlePath template demo. It intentionally uses a comic-book domain so each framework feature can be shown without business noise.

## What This Demo Covers

- Recommended `BaseEntity` domain entities with `CId` identifiers: `Hero`, `Villain`, `Team`, `Skill` and `Incident`.
- A legacy entity, `LegacyCaseFile`, configured with an integer-backed `CId` through `HeroesIdentifierProfile`.
- Entity Framework Core with SQLite and clean `AppDbContext`; entity shape lives in `Configurations`.
- TurtlePath Automations for CRUD happy paths where handlers do not need custom code.
- Custom command handlers for incident assignment and resolution, including handlers that do not inherit from TurtlePath handler bases.
- Feature services that encapsulate EF, background work, and ADO.NET read models instead of leaking those tools into controllers, handlers or jobs.
- A custom paged query handler that inherits from TurtlePath and overrides the query path.
- OctoMap profiles for command-to-entity and entity-to-response projections.
- Crabalidator validators for create, update, patch and assignment commands.
- DataScorpio query profiles with aliases, search, default sort, custom filters and custom sorts.
- Spider controllers that call `Spider.DefaultSend(...)` instead of using `IMediator` directly.
- Cross-cutting hooks for auditing queries and saved commands.
- Feature-specific hooks for normalization and incident defaults.
- Feature services under `Business/Services`, including audit, incident workflows, background backlog work, team reputation, ADO.NET reporting and threat scoring.
- TurtlePath Jobs for both one-shot workloads and recurring background jobs.
- Template testing examples for handler unit tests, Pelican integration, SQLite and DataScorpio.

## Domain Layer

The domain project contains the model that business code owns:

- `Hero` demonstrates the recommended `BaseEntity` path.
- `Villain` demonstrates patch automation and custom DataScorpio sorting.
- `Team` demonstrates shared navigation and cron-maintained calculations.
- `Skill` demonstrates two create commands mapped into the same entity.
- `Incident` demonstrates automation for reporting, custom handlers for assignment/resolution and cron assignment.
- `LegacyCaseFile` demonstrates a legacy table where the database key is an `int`, while app code still sees `CId`.

The legacy identifier behavior is configured in:

```text
src/Heroes.Service.Domain/Identifier/HeroesIdentifierProfile.cs
```

The API composition root discovers it with:

```csharp
.UseCIdProfiles(typeof(HeroesIdentifierProfile).Assembly)
```

## Persistence Layer

The demo uses SQLite to be runnable without SQL Server or PostgreSQL:

```text
src/Heroes.Service.Api/appsettings.json
```

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=heroes-showcase.db"
  }
}
```

`AppDbContext` stays clean. The entity model is configured through:

```text
src/Heroes.Service.Persistence/Configurations
```

This follows the template recommendation: use configurations instead of growing the DbContext with `DbSet` properties and entity shape decisions.

The API creates the SQLite schema automatically on startup through:

```text
src/Heroes.Service.Api/DependencyInjection/DatabaseExtensions.cs
```

That keeps the showcase runnable from a clean folder. Production services should normally use migrations instead.

Development also enables `Demo:SeedOnStartup`, which runs `SeedHeroesUniverseJob` after schema creation so `GET /api/v1/heroes` and `GET /api/v1/teams` return meaningful data immediately. Disable it by setting:

```json
{
  "Demo": {
    "SeedOnStartup": false
  }
}
```

## Messaging And Outbox

The demo registers Pigeon and the EF outbox in:

```text
src/Heroes.Service.Api/DependencyInjection/MessagingExtensions.cs
```

`appsettings.json` keeps the production-style EF outbox configuration enabled. `appsettings.Development.json` disables the outbox dispatcher so the SQLite local showcase can run without Azure Service Bus or durable background dispatch noise.

To enable Service Bus transport, set:

```json
{
  "Pigeon": {
    "MessageBrokers": {
      "AzureServiceBus": {
        "ConnectionString": "<your-connection-string>"
      }
    },
    "Outbox": {
      "Enabled": true
    }
  }
}
```

Publishing is configured with `AmbientTransactionPublishBehavior.SuppressTransaction`, which is the recommended setting when Spider owns the transaction boundary.

## Automations

Automations live inside each feature:

```text
src/Heroes.Service.Business/Heroes/Automations/HeroAutomationProfile.cs
src/Heroes.Service.Business/Villains/Automations/VillainAutomationProfile.cs
src/Heroes.Service.Business/Teams/Automations/TeamAutomationProfile.cs
src/Heroes.Service.Business/Skills/Automations/SkillAutomationProfile.cs
src/Heroes.Service.Business/Incidents/Automations/IncidentAutomationProfile.cs
```

Examples shown:

- `ToCreate<TRequest, TResponse>()`
- `ToUpdate<TRequest, TResponse>()`
- `ToPatch<TRequest>()`
- `ToPatch<TRequest, TResponse>()`
- `ToGetById<TQuery, TResponse>()`
- `ToGetPaged<TQuery, TResponse>()`

`SkillAutomationProfile` shows that the same entity can expose multiple create flows:

- `CreateHeroSkillRequest`
- `CreateVillainSkillRequest`

## Custom Handlers

Use automations for straightforward CRUD. Use handlers when the request must coordinate business rules, services or explicit persistence.

The demo includes a TurtlePath-based query override:

```text
src/Heroes.Service.Business/Heroes/Queries/GetPagedHeroesQueryHandler.cs
```

That handler inherits from `GetPagedInfoQueryHandler` and overrides query composition to apply an optional `TeamId` filter.

The demo also includes fully custom Pelican handlers:

```text
src/Heroes.Service.Business/Incidents/Handlers/AssignIncidentCommandHandler.cs
src/Heroes.Service.Business/Incidents/Handlers/ResolveIncidentCommandHandler.cs
```

Those handlers do not inherit from TurtlePath bases because the flow coordinates a custom workflow. They still stay thin: persistence and rule orchestration live behind `IIncidentWorkflowService`, while the handler only invokes the use case, adds audit context and maps the response.

The demo also includes a custom read model handler:

```text
src/Heroes.Service.Business/Heroes/Queries/GetHeroOperationsReportQueryHandler.cs
src/Heroes.Service.Business/Heroes/Services/OperationsReport/IHeroOperationsReportService.cs
src/Heroes.Service.Business/Heroes/Services/OperationsReport/HeroOperationsReportService.cs
src/Heroes.Service.Persistence/Repositories/Heroes/IHeroOperationsReadRepository.cs
src/Heroes.Service.Persistence/Repositories/Heroes/AdoHeroOperationsReadRepository.cs
```

This shows a clean escape hatch for ADO.NET or Dapper-style reads. The handler remains a normal Pelican request handler, Business owns the feature service, and the storage-specific SQL is isolated in Persistence.

## Requests And Responses

Requests keep the `Request` suffix even when they are command messages:

```text
CreateHeroRequest
UpdateVillainRequest
AssignIncidentRequest
ResolveIncidentRequest
```

Query messages use the `Query` suffix:

```text
GetHeroByIdQuery
GetPagedHeroesQuery
GetIncidentByIdQuery
GetPagedIncidentsQuery
```

Responses describe handler output:

```text
HeroResponse
VillainResponse
TeamResponse
IncidentResponse
SkillResponse
```

## Mapping With OctoMap

Each feature owns its mapping profile:

```text
src/Heroes.Service.Business/Heroes/Mappings/HeroMappingProfile.cs
src/Heroes.Service.Business/Villains/Mappings/VillainMappingProfile.cs
src/Heroes.Service.Business/Teams/Mappings/TeamMappingProfile.cs
src/Heroes.Service.Business/Skills/Mappings/SkillMappingProfile.cs
src/Heroes.Service.Business/Incidents/Mappings/IncidentMappingProfile.cs
```

The profiles show:

- request-to-entity mapping for create and update commands
- entity-to-response mapping
- navigation projection such as `TeamName`, `HeroAlias` and `VillainAlias`
- command-specific enrichment such as setting `OwnerAlignment`

## Validation With Crabalidator

Validators live inside each feature:

```text
src/Heroes.Service.Business/Heroes/Validators/HeroRequestValidators.cs
src/Heroes.Service.Business/Incidents/Validators/IncidentRequestValidators.cs
```

The demo validates required text, string length, numeric ranges and required identifiers. TurtlePath command steps call `IValidatorAdapter`, so handlers and automations stay focused on business flow.

## DataScorpio Filtering

Query profiles live in `Querying` folders:

```text
src/Heroes.Service.Business/Heroes/Querying/HeroQueryProfile.cs
src/Heroes.Service.Business/Villains/Querying/VillainQueryProfile.cs
src/Heroes.Service.Business/Incidents/Querying/IncidentQueryProfile.cs
```

Examples included:

- aliases: `city`, `alias`, `power`, `threat`, `reported`
- search: hero alias/real name, villain alias, incident title
- default sort: heroes by alias, incidents by reported date
- max page size for incidents
- custom filters: `Elite`, `AtLarge`, `Open`
- custom sort: villain `Danger`

## Spider Controllers

Controllers are plural and REST-focused:

```text
src/Heroes.Service.Api/Controllers/HeroesController.cs
src/Heroes.Service.Api/Controllers/VillainsController.cs
src/Heroes.Service.Api/Controllers/TeamsController.cs
src/Heroes.Service.Api/Controllers/SkillsController.cs
src/Heroes.Service.Api/Controllers/IncidentsController.cs
```

They call:

```csharp
Spider.DefaultSend(request, cancellationToken)
```

The bridge is implemented here:

```text
TurtlePath.Spider
```

That keeps controllers aligned with Spider boundaries while Pelican remains the handler dispatcher.

## Hooks

Cross-cutting hooks:

```text
src/Heroes.Service.Business/Services/Audit/AuditAfterSaveHook.cs
src/Heroes.Service.Business/Services/Audit/QueryAuditHook.cs
```

Feature-specific hooks:

```text
src/Heroes.Service.Business/Heroes/Hooks/NormalizeHeroAfterMapHook.cs
src/Heroes.Service.Business/Incidents/Hooks/IncidentDefaultsBeforeSaveHook.cs
```

Use hooks when the same handler path is still correct, but one stage needs extension:

- normalize mapped entity values
- set default dates/statuses
- audit saves
- audit query execution
- push metrics without changing handlers

## Services

Feature services live inside their owning feature:

```text
Heroes/Services/OperationsReport
Incidents/Services/Assignment
Incidents/Services/Backlog
Incidents/Services/ThreatScoring
Incidents/Services/Workflow
Teams/Services/Reputation
Jobs/Services/Universe
```

Use `Services/<Capability>` so the interface, implementation and supporting classes for one dependency are easy to extract later.

Only cross-cutting services live under `Business/Services`:

```text
Services/Audit
```

The template keeps custom dependency registration in:

```text
src/Heroes.Service.Api/DependencyInjection/CustomContainerExtensions.cs
```

That extension is chained after defaults, so project dependencies do not pollute template defaults.

Handlers and jobs should depend on feature services when the workflow needs custom persistence, external SDKs, ADO.NET, Dapper, blob storage or vendor APIs. That keeps the handler shape consistent and makes extreme customization easier to test.

Repository contracts and implementations that are storage-specific live in `Persistence/Repositories`. Business should not reference Persistence, and Persistence should not reference Business.

## Jobs

The demo includes one-shot and recurring jobs:

```text
src/Heroes.Service.Business/Jobs/SeedHeroesUniverseJob.cs
src/Heroes.Service.Business/Jobs/AutoAssignOpenIncidentsJob.cs
src/Heroes.Service.Business/Jobs/RecalculateTeamReputationJob.cs
```

`SeedHeroesUniverseJob` is a one-shot job. It delegates seeding to `IHeroesUniverseSeeder`, which keeps EF access out of the job and makes the job a small orchestration boundary.

`AutoAssignOpenIncidentsJob` is a recurring cron-style job. It delegates the backlog workflow to `IIncidentBacklogService`, which can use EF today and be replaced by another implementation later.

`RecalculateTeamReputationJob` is another recurring job. It delegates calculations to `ITeamReputationService`, avoiding direct `IDbContext` usage inside the job class.

The registration shows multiple jobs in the same host:

```text
src/Heroes.Service.Api/DependencyInjection/CustomContainerExtensions.cs
```

## Testing

The test project includes:

- composition tests for template defaults
- TurtlePath unit-style handler tests without hand-written mocks
- Pelican integration tests
- SQLite-backed EF tests
- DataScorpio query tests
- transaction boundary tests

Start here:

```text
tests/Heroes.Service.Tests/TurtlePathTestingExamplesTests.cs
tests/Heroes.Service.Tests/TemplateCompositionTests.cs
TurtlePath.Spider.Transactions package tests cover the reusable transaction boundary.
tests/Heroes.Service.Tests/HeroOperationsReadModelTests.cs
```

For a guided index of the specific scenarios implemented in this demo, read:

```text
HOW_TO_USE_CASES.md
```

## Suggested Walkthrough

1. Read the domain entities first.
2. Read the EF configurations to see how persistence stays outside the DbContext.
3. Read `HeroAutomationProfile` for a simple automation flow.
4. Read `SkillAutomationProfile` for multiple create commands against one entity.
5. Read `GetPagedHeroesQueryHandler` for a TurtlePath handler override.
6. Read the incident handlers for full custom Pelican handlers.
7. Read the DataScorpio profiles for query customization.
8. Read the hooks to see where cross-cutting behavior belongs.
9. Read the jobs to see one-shot and recurring work.
10. Read the tests to understand how to validate handlers and integrations.
