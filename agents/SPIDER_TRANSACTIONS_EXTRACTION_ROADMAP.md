# Spider Transactions Extraction Roadmap

## Objective

Move the transaction boundary out of the template and into reusable TurtlePath packages while preserving the dependency direction of the generated applications.

The implementation must not reference Entity Framework Core. The transaction boundary creates an ambient `System.Transactions.TransactionScope`; persistence technologies participate in that scope independently.

## Package Design

### `TurtlePath.Spider`

Keep the existing Spider integration package as the shared TurtlePath-to-Spider integration surface. It owns the reusable Spider bridge extensions and any common registration primitives required by TurtlePath integrations. It must depend on TurtlePath and Spider, but not on EF Core.

### `TurtlePath.Spider.Transactions`

Create a dedicated package for the transaction implementation. It owns:

- `TransactionBoundaryOptions`.
- Transaction boundary profiles and discovery contracts.
- Transaction request filtering and cached request discovery.
- `SkipTransactionBoundaryAttribute`.
- `TransactionExecutionBoundary`.
- Chainable dependency-injection extensions.

Its dependencies are `TurtlePath.Spider`, `Spider.Pipelines`, `System.Transactions`, and the required Microsoft extensions abstractions. It must not reference `Microsoft.EntityFrameworkCore`.

## Execution Steps

1. Preserve the current template and Heroes folder reorganization under `Boundaries/Transactions`, update namespaces, references, tests, and documentation, and verify both generated solutions.
2. Add `TurtlePath.Spider.Transactions` to the solution with package metadata, documentation, shared versioning, and focused unit tests for filtering, profiles, scope completion, faults, cancellation, and nested scopes.
3. Move the transaction implementation from the template into the new package and adapt it to the package namespace without changing behavior.
4. Add the chainable registration API and profile discovery to the package. The registration must compose with `TurtlePath.Spider` and avoid requiring feature-specific registrations in the template.
5. Replace the template and Heroes local implementations with package references and package registration. Remove duplicated transaction source files and update their tests to test the package contract.
6. Update the English and Spanish guides, project shape examples, README files, package metadata, and changelog entries to document the new package and dependency boundaries.
7. Run package tests, template tests, Heroes tests, package builds, dependency checks, and `git diff --check`. Review the final dependency graph to confirm no transaction package references EF Core.

## Dependency Direction

```text
TurtlePath.Domain -> TurtlePath.Persistence -> TurtlePath.Business -> API

TurtlePath.Spider
  -> TurtlePath
  -> Spider.Pipelines

TurtlePath.Spider.Transactions
  -> TurtlePath.Spider
  -> Spider.Pipelines
  -> System.Transactions
  -> Microsoft.Extensions.* abstractions
```

EF Core remains isolated in `TurtlePath.EntityFrameworkCore` and participates in the ambient transaction through its own provider integration.

## Compatibility Rules

- Preserve the existing transaction behavior and configuration keys.
- Preserve profile discovery and request filtering semantics.
- Preserve the public `SkipTransactionBoundaryAttribute` behavior.
- Keep the template's public setup chain concise and extensible.
- Do not introduce a generic TurtlePath boundary abstraction that duplicates Spider's boundary model.
