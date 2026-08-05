# Release

TurtlePath follows the OctoMap release workflow shape.

1. Update `CHANGELOG.md` with a `## [vX.Y.Z]` section.
2. Create a branch named `releases/vX.Y.Z`.
3. Run the `Release to NuGet` workflow manually.

The workflow validates the branch name and changelog entry, builds, tests, packs, tags, creates a GitHub release, and publishes packages through NuGet trusted publishing.

The solution now contains multiple packable projects. `dotnet pack` should produce package artifacts for:

- `TurtlePath.Abstractions`
- `TurtlePath.Domain`
- `TurtlePath`
- `TurtlePath.EntityFrameworkCore`
- `TurtlePath.Automations`
- `TurtlePath.OctoMap`
- `TurtlePath.AutoMapper`
- `TurtlePath.Crabalidator`
- `TurtlePath.FluentValidation`
- `TurtlePath.Sieve`
- `TurtlePath.Analyzers`

