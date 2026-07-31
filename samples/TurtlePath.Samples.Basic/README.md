# TurtlePath Basic Sample

This sample exercises TurtlePath through Pelican mediator dispatch, command handlers, hooks, CId profiles, and EF Core with SQLite.

`TenantOrderKey` demonstrates a custom C# identifier value stored as a single database string column through `CId`. It is not modeled as an EF Core composite primary key. A real composite key needs entity-aware mapping across multiple properties/columns, which is intentionally different from the simple `CId<TValue, TDbValue>` converter path.
