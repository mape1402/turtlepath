# TurtlePath Heroes Showcase Template

Creates a complete TurtlePath demo service with a Heroes domain.

## Install

```powershell
dotnet new install TurtlePath.Template.HeroesShowcase
```

## Create

```powershell
dotnet new turtlepath-heroes-showcase -n Heroes.Service
```

## Select TurtlePath Package Version

By default, the generated demo uses the TurtlePath package version tested with this demo template release.

To generate the demo with a specific TurtlePath package version, pass:

```powershell
dotnet new turtlepath-heroes-showcase -n Heroes.Service --turtlepath-version 1.6.0
```

The generated solution centralizes TurtlePath package references through `TurtlePathVersion` in `Directory.Build.targets`.

The generated solution demonstrates:

- TurtlePath automations for happy-path commands and queries.
- Custom command and query handlers for specialized workflows.
- CId identifiers backed by ULID values.
- Entity Framework Core with SQLite.
- DataScorpio filtering and sorting profiles.
- OctoMap mapping profiles.
- Crabalidator validators.
- Spider pipeline calls to Pelican handlers through TurtlePath.Spider.
- One-shot and recurring jobs.
- Exception handling setup.
- Optional Pigeon and EventSourcing extension points.
- Unit and integration tests using TurtlePath testing helpers.
