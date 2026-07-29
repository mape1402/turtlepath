# TurtlePath Architecture

TurtlePath contains the reusable application-handler foundation extracted from the template.

The library is intentionally organized around the same boundaries as the original template core:

- `TurtlePath.Identifier` provides opaque identifier primitives.
- `TurtlePath.Domain` provides domain entity contracts.
- `TurtlePath.Application` provides Pelican handler base classes, hooks, request/response primitives, exceptions, and adapter contracts.
- `TurtlePath.Persistence.Abstractions` provides provider-neutral read/write contracts.
- `TurtlePath.EntityFrameworkCore` provides EF Core storage adapters and entity configuration helpers.
- `TurtlePath.OctoMap`, `TurtlePath.Crabalidator`, and `TurtlePath.Sieve` provide optional stack adapters.
- `TurtlePath.AspNetCore` and `TurtlePath.Swagger` provide web-facing integrations.

Applications keep their domain-specific handlers, entities, validators, maps, controllers, and DbContext. TurtlePath supplies the common behavior those pieces build on.

## Dependency Direction

The intended dependency flow is:

```text
Identifier
Domain -> Identifier
Persistence.Abstractions -> Domain
Application -> Domain, Persistence.Abstractions
EntityFrameworkCore -> Application, Domain, Persistence.Abstractions
OctoMap -> Application
Crabalidator -> Application
Sieve -> Persistence.Abstractions
AspNetCore -> Identifier, Application
Swagger -> Identifier
TurtlePath -> all focused packages for compatibility
```
