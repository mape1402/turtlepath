# TurtlePath Basic Sample

This sample exercises TurtlePath through Pelican mediator dispatch, automation profiles, manual handler escape hatches, hooks, scalar CId profiles, Sieve criteria, and EF Core with SQLite.

Covered flows:

- `TurtlePath.Automations` for Customer create, update, patch, get by id, and paged query happy paths.
- `IPatchAction<TEntity>` for request-owned patch behavior.
- Manual command and query handlers for custom flows that need full control.
- `DeleteCommandHandler` with a custom delete response.
- `GenericCreateCommandHandler` and `GenericGetByIdQueryHandler` for an `IEntity<int>` legacy model.
- Hook ordering for mapping, id assignment, and audit logging.
- Configured CId conversion for the default `Guid` path and a per-entity `int` database representation.
- Sieve string filters and sorts backed by the storage criteria adapter.

`CId` support is intentionally scalar in the current pipeline: one domain value maps to one database value. Composite keys require entity-aware mapping and are not part of the current CId converter support.
