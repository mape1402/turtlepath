# TurtlePath Architecture

TurtlePath contains the reusable application-handler foundation extracted from the template.

The library is intentionally organized around the same boundaries as the original template core:

- `Core/Commands` and `Core/Queries` provide Pelican handler base classes.
- `Core/Hooks` defines before/after extension points and ordered hook execution.
- `Core/Services` defines abstractions for mapping, validation, and persistence operations.
- `Core/Infrastructure` provides default adapters for OctoMap, Crabalidator, Sieve, and EF Core.
- `Contracts` and `Identifier` provide entity and identifier primitives.
- `Persistence` provides EF-facing abstractions and base configurations.

Applications keep their domain-specific handlers, entities, validators, maps, controllers, and DbContext. TurtlePath supplies the common behavior those pieces build on.
