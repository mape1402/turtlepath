# TurtlePath Architecture

TurtlePath contains the reusable application-handler foundation extracted from the template.

The library is intentionally organized around the same boundaries as the original template core:

- `TurtlePath.Abstractions` provides provider-neutral mapping, validation, and persistence contracts.
- `TurtlePath.Domain` provides opaque identifier primitives, JSON converters for identifiers, and domain entity contracts.
- `TurtlePath` provides Pelican handler base classes, hooks, request/response primitives, and application errors.
- `TurtlePath.EntityFrameworkCore` provides EF Core storage adapters and entity configuration helpers.
- `TurtlePath.OctoMap`, `TurtlePath.Crabalidator`, and `TurtlePath.Sieve` provide optional stack adapters.

Applications keep their domain-specific handlers, entities, validators, maps, controllers, and DbContext. TurtlePath supplies the common behavior those pieces build on.

## Dependency Direction

The intended dependency flow is:

```text
Abstractions -> Domain
TurtlePath -> Abstractions, Domain
EntityFrameworkCore -> Abstractions, Domain
OctoMap -> Abstractions
Crabalidator -> Abstractions
Sieve -> Abstractions
```


