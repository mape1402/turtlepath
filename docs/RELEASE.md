# Release

TurtlePath has separate release tracks. Keep each track focused so runtime packages, project templates, and Studio releases do not share the same changelog noise.

## Runtime Libraries

Use this track for projects under `src/`.

1. Update `CHANGELOG.md` with a `## [vX.Y.Z]` section.
2. Update `.release` with `vX.Y.Z`.
3. Merge to `main`, or create a branch named `releases/vX.Y.Z` and run the workflow manually.

The `Build and Release` workflow validates the marker and changelog entry, builds, tests, packs projects under `src/`, tags the release, creates a GitHub release, and publishes packages through NuGet trusted publishing.

Runtime package artifacts include:

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
- `TurtlePath.DataScorpio`
- `TurtlePath.EventSourcing`
- `TurtlePath.ExceptionHandling`
- `TurtlePath.ExceptionHandling.AspNetCore`
- `TurtlePath.ExceptionHandling.Consumers`
- `TurtlePath.ExceptionHandling.Workers`
- `TurtlePath.Jobs`
- `TurtlePath.Spider`
- `TurtlePath.Studio.Tool`
- `TurtlePath.Testing`
- `TurtlePath.Testing.EntityFrameworkCore`
- `TurtlePath.Testing.EventSourcing`
- `TurtlePath.Testing.Integration`

## Service Template

Use this track for `templates/TurtlePath.Template`.

1. Update `TEMPLATE_CHANGELOG.md` with a `## [template-vX.Y.Z]` section.
2. Update `.template.release` with `template-vX.Y.Z`.
3. Merge to `main`, or create a branch named `template-releases/template-vX.Y.Z` and run the workflow manually.

The `Template Release` workflow validates the generated template solution, stamps `turtlepath.template.json` with `X.Y.Z`, packs only `TurtlePath.Template`, creates the `template-vX.Y.Z` GitHub release, and publishes the template package through NuGet trusted publishing.

## Studio

Use this track for the desktop Studio app.

1. Update `STUDIO_CHANGELOG.md` with a `## X.Y.Z` section.
2. Update `.studio.release` with `studio-vX.Y.Z`.
3. Merge to `main`, or run the `Studio Release` workflow manually.

The `Studio Release` workflow builds the Windows app, injects the semantic version from `.studio.release`, publishes the updater helper, creates `TurtlePath.Studio.win-x64.zip`, generates `studio.manifest.json`, and attaches both files to the `studio-vX.Y.Z` GitHub release.

