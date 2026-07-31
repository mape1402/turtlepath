# TurtlePath Basic Sample

This sample exercises TurtlePath through Pelican mediator dispatch, command handlers, hooks, scalar CId profiles, and EF Core with SQLite.

`CId` support is intentionally scalar in the current pipeline: one domain value maps to one database value. Composite keys require entity-aware mapping and are not part of the current CId converter support.
