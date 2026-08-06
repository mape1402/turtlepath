# TurtlePath Basic Sample

This sample exercises TurtlePath through Pelican mediator dispatch, automation profiles, attribute automations, manual handler escape hatches, hooks, scalar CId profiles, Sieve criteria, EF Core with SQLite, and the recommended Elysium adapters: OctoMap and Crabalidator.

Run it with:

```bash
dotnet run --project samples/TurtlePath.Samples.Basic/TurtlePath.Samples.Basic.csproj
```

Covered flows:

- `TurtlePath.Automations` for Customer create, update, patch, get by id, and paged query happy paths.
- Attribute-based automations for CatalogItem create, update, get by id, and delete.
- Automated LegacyInvoice create, update, and get by id using `BaseEntity`/`CId` while storing the id with an entity-specific `int` configuration.
- `TurtlePath.OctoMap` backed by `CommerceMappingProfile` for request/entity/response mapping.
- `TurtlePath.Crabalidator` backed by request validators under `Application/Validation`.
- `IPatchAction<TEntity>` for request-owned patch behavior.
- Manual command and query handlers for custom flows that need full control.
- `DeleteCommandHandler` with a custom delete response.
- `GenericCreateCommandHandler` and `GenericGetByIdQueryHandler` for an `IEntity<int>` legacy model.
- Hook ordering for mapping, id assignment, and audit logging.
- Configured CId conversion for the default `Guid` path and a per-entity `int` database representation.
- Sieve string filters and sorts backed by the storage criteria adapter.
- `TurtlePath.ExceptionHandling` for transport-neutral exception descriptors.
- `ExceptionHandlingProfile` to keep exception mappings outside dependency registration.
- `TurtlePath.ExceptionHandling.AspNetCore` for HTTP status and ProblemDetails projection.
- `TurtlePath.ExceptionHandling.Consumers` for message consumer retry/complete behavior.
- `TurtlePath.ExceptionHandling.Workers` for background services and Kubernetes-style cron jobs.
- `TurtlePath.Jobs` for one-shot Kubernetes jobs and multiple recurring cron-style background jobs.

`CId` support is intentionally scalar in the current pipeline: one domain value maps to one database value. Composite keys require entity-aware mapping and are not part of the current CId converter support.
