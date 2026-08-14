# Heroes Showcase How To Index

This document maps each important customization scenario in the Heroes demo to the code that implements it. Read it as a practical guide for moving from the happy path to deeper custom behavior without breaking the template structure.

## Index

- [How To Use Automations For Straight CRUD](#how-to-use-automations-for-straight-crud)
- [How To Extend A Paged Query Handler](#how-to-extend-a-paged-query-handler)
- [How To Create A Custom Handler Without TurtlePath Base Classes](#how-to-create-a-custom-handler-without-turtlepath-base-classes)
- [How To Encapsulate EF Logic Behind A Feature Service](#how-to-encapsulate-ef-logic-behind-a-feature-service)
- [How To Add ADO.NET Or Dapper-Style Reads Cleanly](#how-to-add-adonet-or-dapper-style-reads-cleanly)
- [How To Keep Jobs Thin And Service-Oriented](#how-to-keep-jobs-thin-and-service-oriented)
- [How To Customize DataScorpio Filtering](#how-to-customize-datascorpio-filtering)
- [How To Use OctoMap Profiles](#how-to-use-octomap-profiles)
- [How To Use Crabalidator Validators](#how-to-use-crabalidator-validators)
- [How To Add Hooks](#how-to-add-hooks)
- [How To Use Spider From Controllers](#how-to-use-spider-from-controllers)
- [How To Support Legacy Identifier Shapes](#how-to-support-legacy-identifier-shapes)
- [How To Test The Demo](#how-to-test-the-demo)

## How To Use Automations For Straight CRUD

Use automations when a request maps to an entity, validates, saves, and maps a response without extra orchestration.

Implemented examples:

- `src/Heroes.Service.Business/Heroes/Automations/HeroAutomationProfile.cs`
- `src/Heroes.Service.Business/Villains/Automations/VillainAutomationProfile.cs`
- `src/Heroes.Service.Business/Teams/Automations/TeamAutomationProfile.cs`
- `src/Heroes.Service.Business/Skills/Automations/SkillAutomationProfile.cs`
- `src/Heroes.Service.Business/Incidents/Automations/IncidentAutomationProfile.cs`

The `Skills` feature demonstrates multiple create commands over the same entity: hero skills and villain skills share persistence but expose different request models.

## How To Extend A Paged Query Handler

Use a TurtlePath base handler override when the standard query pipeline is correct but the entity needs extra query composition.

Implemented example:

- `src/Heroes.Service.Business/Heroes/Queries/GetPagedHeroesQuery.cs`
- `src/Heroes.Service.Business/Heroes/Queries/GetPagedHeroesQueryHandler.cs`

The handler adds a `TeamId` filter before the default paging, sorting and DataScorpio behavior runs.

## How To Create A Custom Handler Without TurtlePath Base Classes

Use a custom Pelican handler when the workflow is not a CRUD happy path.

Implemented examples:

- `src/Heroes.Service.Business/Incidents/Handlers/AssignIncidentCommandHandler.cs`
- `src/Heroes.Service.Business/Incidents/Handlers/ResolveIncidentCommandHandler.cs`

These handlers do not inherit from TurtlePath base handlers. They still follow a clean pattern: call a feature service, add request-specific audit context, and map the response.

## How To Encapsulate EF Logic Behind A Feature Service

When a handler or job needs EF-specific work, put that logic in a service. The handler or job should not become a persistence script.

Implemented examples:

- `src/Heroes.Service.Business/Incidents/Services/Workflow/IIncidentWorkflowService.cs`
- `src/Heroes.Service.Business/Incidents/Services/Workflow/IncidentWorkflowService.cs`
- `src/Heroes.Service.Business/Incidents/Services/Backlog/IIncidentBacklogService.cs`
- `src/Heroes.Service.Business/Incidents/Services/Backlog/IncidentBacklogService.cs`
- `src/Heroes.Service.Business/Teams/Services/Reputation/ITeamReputationService.cs`
- `src/Heroes.Service.Business/Teams/Services/Reputation/TeamReputationService.cs`
- `src/Heroes.Service.Business/Jobs/Services/Universe/IHeroesUniverseSeeder.cs`
- `src/Heroes.Service.Business/Jobs/Services/Universe/HeroesUniverseSeeder.cs`

This keeps custom handlers small and gives tests a focused seam for business behavior.

## How To Add ADO.NET Or Dapper-Style Reads Cleanly

Use a feature read service when a query needs optimized SQL, reporting shape, vendor-specific SQL, Dapper, or ADO.NET.

Implemented examples:

- `src/Heroes.Service.Business/Heroes/Queries/GetHeroOperationsReportQuery.cs`
- `src/Heroes.Service.Business/Heroes/Queries/GetHeroOperationsReportQueryHandler.cs`
- `src/Heroes.Service.Business/Heroes/Services/OperationsReport/IHeroOperationsReportService.cs`
- `src/Heroes.Service.Business/Heroes/Services/OperationsReport/HeroOperationsReportService.cs`
- `src/Heroes.Service.Persistence/Repositories/Heroes/IHeroOperationsReadRepository.cs`
- `src/Heroes.Service.Persistence/Repositories/Heroes/HeroOperationsReadRow.cs`
- `src/Heroes.Service.Persistence/Repositories/Heroes/AdoHeroOperationsReadRepository.cs`

The controller still sends a Pelican query through Spider. The handler delegates to a feature service in Business. The service reads from a Persistence-owned repository contract. The SQL stays inside the persistence implementation, where storage-specific code belongs.

Endpoint:

```http
GET /api/v1/heroes/operations-report
```

## How To Keep Jobs Thin And Service-Oriented

Jobs should describe scheduling and orchestration. Services should own EF, external APIs, files, queues or other infrastructure work.

Implemented examples:

- `src/Heroes.Service.Business/Jobs/SeedHeroesUniverseJob.cs` delegates to `IHeroesUniverseSeeder`
- `src/Heroes.Service.Business/Jobs/AutoAssignOpenIncidentsJob.cs` delegates to `IIncidentBacklogService`
- `src/Heroes.Service.Business/Jobs/RecalculateTeamReputationJob.cs` delegates to `ITeamReputationService`

This pattern works for one-shot Kubernetes jobs and recurring background jobs.

## How To Customize DataScorpio Filtering

Use query profiles to keep filter and sort names stable for API users.

Implemented examples:

- `src/Heroes.Service.Business/Heroes/Querying/HeroQueryProfile.cs`
- `src/Heroes.Service.Business/Villains/Querying/VillainQueryProfile.cs`
- `src/Heroes.Service.Business/Incidents/Querying/IncidentQueryProfile.cs`

The demo includes aliases, search, default sorts, custom filters and custom sorts.

## How To Use OctoMap Profiles

Use mapping profiles per feature. Keep mappings close to the requests and responses they serve.

Implemented examples:

- `src/Heroes.Service.Business/Heroes/Mappings/HeroMappingProfile.cs`
- `src/Heroes.Service.Business/Villains/Mappings/VillainMappingProfile.cs`
- `src/Heroes.Service.Business/Teams/Mappings/TeamMappingProfile.cs`
- `src/Heroes.Service.Business/Skills/Mappings/SkillMappingProfile.cs`
- `src/Heroes.Service.Business/Incidents/Mappings/IncidentMappingProfile.cs`

The mappings show create/update requests, response projections and navigation-derived response fields.

## How To Use Crabalidator Validators

Use validators for request shape and input rules before the handler flow reaches persistence.

Implemented examples:

- `src/Heroes.Service.Business/Heroes/Validators/HeroRequestValidators.cs`
- `src/Heroes.Service.Business/Villains/Validators/VillainRequestValidators.cs`
- `src/Heroes.Service.Business/Teams/Validators/TeamRequestValidators.cs`
- `src/Heroes.Service.Business/Skills/Validators/SkillRequestValidators.cs`
- `src/Heroes.Service.Business/Incidents/Validators/IncidentRequestValidators.cs`

## How To Add Hooks

Use hooks when the normal handler or automation path is correct, but one stage needs extension.

Cross-cutting examples:

- `src/Heroes.Service.Business/Services/Audit/AuditAfterSaveHook.cs`
- `src/Heroes.Service.Business/Services/Audit/QueryAuditHook.cs`

Feature-specific examples:

- `src/Heroes.Service.Business/Heroes/Hooks/NormalizeHeroAfterMapHook.cs`
- `src/Heroes.Service.Business/Incidents/Hooks/IncidentDefaultsBeforeSaveHook.cs`

## How To Use Spider From Controllers

Controllers use Spider as the application boundary and let Pelican dispatch the request.

Implemented examples:

- `src/Heroes.Service.Api/Controllers/HeroesController.cs`
- `src/Heroes.Service.Api/Controllers/IncidentsController.cs`
- `src/Heroes.Service.Api/Controllers/VillainsController.cs`

The call shape is:

```csharp
return Spider.DefaultSend<TRequest, TResponse>(request, cancellationToken);
```

## How To Support Legacy Identifier Shapes

Use a CId profile when one entity cannot follow the standard identifier shape.

Implemented example:

- `src/Heroes.Service.Domain/Identifier/HeroesIdentifierProfile.cs`
- `src/Heroes.Service.Persistence/Configurations/LegacyCaseFileConfiguration.cs`

The app can keep using `CId` while the database stores a legacy integer key for that entity.

## How To Test The Demo

The test project demonstrates unit-style and integration-style coverage.

Implemented examples:

- `tests/Heroes.Service.Tests/HeroesEntityTests.cs`
- `tests/Heroes.Service.Tests/VillainsEntityTests.cs`
- `tests/Heroes.Service.Tests/TeamsEntityTests.cs`
- `tests/Heroes.Service.Tests/SkillsEntityTests.cs`
- `tests/Heroes.Service.Tests/IncidentsEntityTests.cs`
- `tests/Heroes.Service.Tests/JobsBusinessTests.cs`
- `tests/Heroes.Service.Tests/HeroOperationsReadModelTests.cs`
- `tests/Heroes.Service.Tests/SpiderPelicanBridgeTests.cs`
- `TurtlePath.Spider.Transactions` package tests cover the reusable transaction boundary.

The tests use the real dependency graph, SQLite, Pelican, Spider bridge helpers and TurtlePath testing support so users can copy the same testing shape into their generated service.
