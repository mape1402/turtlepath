# TurtlePath Architecture

TurtlePath contains the reusable application-handler foundation extracted from the template.

The library is intentionally organized around the same boundaries as the original template core:

- `TurtlePath.Abstractions` provides provider-neutral mapping, validation, and persistence contracts.
- `TurtlePath.Identifier` provides opaque identifier primitives.
- `TurtlePath.Identifier.EntityFrameworkCore` provides EF Core integration for identifiers.
- `TurtlePath.Domain` provides domain entity contracts.
- `TurtlePath.Application` provides Pelican handler base classes, hooks, request/response primitives, and application errors.
- `TurtlePath.EntityFrameworkCore` provides EF Core storage adapters and entity configuration helpers.
- `TurtlePath.OctoMap`, `TurtlePath.Crabalidator`, and `TurtlePath.Sieve` provide optional stack adapters.
- `TurtlePath.AspNetCore` and `TurtlePath.Swagger` provide web-facing integrations.

Applications keep their domain-specific handlers, entities, validators, maps, controllers, and DbContext. TurtlePath supplies the common behavior those pieces build on.

## Dependency Direction

The intended dependency flow is:

```text
Identifier
Identifier.EntityFrameworkCore -> Identifier
Domain -> Identifier
Abstractions -> Domain
Application -> Abstractions, Domain, Identifier
EntityFrameworkCore -> Abstractions, Domain, Identifier, Identifier.EntityFrameworkCore
OctoMap -> Abstractions
Crabalidator -> Abstractions
Sieve -> Abstractions
AspNetCore -> Identifier, Application
Swagger -> Identifier
TurtlePath -> all focused packages for compatibility
```
