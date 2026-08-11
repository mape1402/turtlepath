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
