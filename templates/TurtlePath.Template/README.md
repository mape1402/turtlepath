# TurtlePath.Template

`TurtlePath.Template` is the official `dotnet new` template package for creating TurtlePath services.

## Install

```bash
dotnet new install TurtlePath.Template
```

## Create An API/Consumer Service

```bash
dotnet new turtlepath -n MyService --host api-consumer
```

## Create A Job Service

```bash
dotnet new turtlepath -n MyJob --host job
```

## Select TurtlePath Package Version

By default, the generated project uses the TurtlePath package version tested with this template release.

To generate a project with a specific TurtlePath package version, pass:

```bash
dotnet new turtlepath -n MyService --host api-consumer --turtlepath-version 1.6.3
```

The generated solution centralizes TurtlePath package references through `TurtlePathVersion` in `Directory.Build.targets`.

The current template release has been validated against TurtlePath `1.6.3`.

## Validate A Generated Project

```bash
dotnet restore
dotnet build
dotnet test
```

The generated project includes TurtlePath defaults for handlers, automations, exception handling, jobs, DataScorpio filtering, OctoMap mapping, Crabalidator validation, and testing foundations.

Transaction boundaries are registered through `TurtlePath.Spider.Transactions` using the generated Business and API assemblies explicitly, so test assemblies and unrelated loaded assemblies are not scanned.

The optional Pigeon integration uses Pigeon 2.4.0. When messaging is enabled, consumer throughput can be bounded with `ConfigureConsumerExecution`, using `MaxConcurrency` for parallel handler dispatch and `QueueCapacity` for the internal waiting queue. See the generated `docs/Use Guide_en.md` or `docs/Use Guide_es.md` for the complete configuration example.

Generated services also include `turtlepath.template.json` at the solution root. That file records the `TurtlePath.Template` package version used to create the service.
