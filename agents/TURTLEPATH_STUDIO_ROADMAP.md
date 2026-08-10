# TurtlePath Studio Roadmap

## Vision

TurtlePath Studio is a standalone client for creating, validating, and evolving TurtlePath projects without forcing developers to remember template commands, package names, or setup conventions.

The first version should focus on a reliable project-generation workflow:

- detect the local .NET environment;
- install or update `TurtlePath.Template` from NuGet;
- create API/consumer and job projects from a guided UI;
- run restore, build, and test commands from the client;
- surface command output in a readable execution log;
- provide local documentation and examples for the public TurtlePath API.

Future versions can grow into feature generation, automation scaffolding, event sourcing setup, testing scaffolds, and ButterMorph integration.

## Architecture

The implementation should be split so the UI is only one possible host:

- `TurtlePath.Studio.Abstractions`: contracts, request/response models, environment facts, command execution results, template metadata.
- `TurtlePath.Studio.Application`: use cases such as environment discovery, template installation, project creation, project validation, and demo generation.
- `TurtlePath.Studio.Infrastructure`: process execution, filesystem access, NuGet/dotnet CLI integration, command output streaming.
- `TurtlePath.Studio.Web`: reusable Blazor/Razor UI shell for the wizard, diagnostics, docs, and execution log.
- `TurtlePath.Studio.App`: desktop host, preferably MAUI Blazor Hybrid once the required workload and packaging strategy are confirmed.
- `TurtlePath.Studio.Tests`: unit tests for command composition, validation rules, and application use cases.

The early steps should keep the core independent from MAUI so it can compile in the repository CI and be reused by any future host.

## Public Workflows

### Environment Check

The client should show:

- whether `dotnet` is available;
- installed SDK versions;
- installed `TurtlePath.Template` versions;
- latest available `TurtlePath.Template` version from NuGet;
- whether the current environment can create `net9.0` or `net10.0` projects.

### Template Management

The client should support:

- install `TurtlePath.Template`;
- update `TurtlePath.Template`;
- uninstall stale local versions when safe;
- show the exact command being executed;
- capture standard output, standard error, exit code, duration, and timestamps.

### Project Creation

The first wizard should ask for:

- project name;
- output folder;
- host mode: `api-consumer` or `job`;
- optional validation after creation: restore, build, test;
- optional demo generation once the template supports more presets.

The generated command should be equivalent to:

```powershell
dotnet new turtlepath -n MyService -o C:\work\MyService --host api-consumer
```

or:

```powershell
dotnet new turtlepath -n MyJob -o C:\work\MyJob --host job
```

### Validation

After generation, the client should optionally run:

- `dotnet restore`;
- `dotnet build --configuration Release --no-restore`;
- `dotnet test --configuration Release --no-build`.

Each command should be represented as a structured execution result.

### Documentation

The first documentation surface should include:

- template getting started;
- API/consumer project creation;
- one-shot job project creation;
- testing guide;
- automations guide;
- jobs guide;
- exception handling guide;
- adapters guide for DataScorpio, OctoMap, Crabalidator, Pigeon, Spider, and Pelican.

## Step Plan

### Step 1: Roadmap

Create this roadmap in `agents/` and commit it.

Validation:

- file exists in `agents/`;
- no product code changes are included.

### Step 2: Core Contracts

Create `TurtlePath.Studio.Abstractions` with the core models and interfaces:

- environment discovery contracts;
- command execution contracts;
- template package contracts;
- project creation request/result models;
- validation request/result models.

Validation:

- project builds for `net9.0` and `net10.0`;
- solution includes the project.

### Step 3: Application Use Cases

Create `TurtlePath.Studio.Application` with use cases that orchestrate the contracts:

- inspect environment;
- install or update template;
- create project;
- validate project.

Validation:

- no direct process or filesystem implementation lives here;
- use cases are unit-testable through abstractions.

### Step 4: Infrastructure

Create `TurtlePath.Studio.Infrastructure` with:

- `dotnet` CLI process runner;
- template package installer;
- template package inspector;
- project generator;
- project validator.

Validation:

- command composition is deterministic;
- integration tests can use a temporary directory;
- process output is captured structurally.

### Step 5: Tests

Create `TurtlePath.Studio.Tests` covering:

- generated command arguments;
- invalid project names and paths;
- host mode mapping;
- validation command sequence;
- template manager behavior with mocked process execution.

Validation:

- tests pass locally for `net9.0` and `net10.0`.

### Step 6: Web UI Shell

Create `TurtlePath.Studio.Web` as a reusable Blazor/Razor UI surface:

- environment status panel;
- template management panel;
- new project wizard;
- execution log viewer;
- documentation landing view.

Validation:

- project compiles without requiring MAUI workloads;
- UI components depend on application abstractions, not process APIs.

### Step 7: Desktop Host

Create `TurtlePath.Studio.App` after validating MAUI workload availability and release packaging expectations.

Validation:

- app can run locally;
- app hosts the web shell;
- app can execute the same project creation workflow.

### Step 8: Packaged Distribution

Prepare packaging once the host is stable:

- Windows standalone artifact;
- template auto-install on first run;
- local docs bundled into the app;
- version display for Studio, TurtlePath.Template, and installed .NET SDK.

Validation:

- app runs on a clean machine with .NET available;
- app can install the template and generate a project.

## Commit Strategy

Each step should be committed independently:

1. `Add TurtlePath Studio roadmap`
2. `Add TurtlePath Studio contracts`
3. `Add TurtlePath Studio application use cases`
4. `Add TurtlePath Studio infrastructure`
5. `Add TurtlePath Studio tests`
6. `Add TurtlePath Studio web shell`
7. `Add TurtlePath Studio desktop host`
8. `Prepare TurtlePath Studio distribution`

## Open Decisions

- Final product name: `TurtlePath Studio`, `TurtlePath Workbench`, or another name.
- Whether the first desktop host should be MAUI Blazor Hybrid or a local web host with a native wrapper.
- Whether Studio ships inside this repository permanently or later moves into a dedicated repository.
- Whether feature generation should call `dotnet new` item templates or use a richer generation engine.
- How much generated-project customization should live in `TurtlePath.Template` versus Studio.
