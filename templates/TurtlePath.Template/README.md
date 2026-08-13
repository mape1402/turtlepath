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

## Validate A Generated Project

```bash
dotnet restore
dotnet build
dotnet test
```

The generated project includes TurtlePath defaults for handlers, automations, exception handling, jobs, DataScorpio filtering, OctoMap mapping, Crabalidator validation, and testing foundations.

Generated services also include `turtlepath.template.json` at the solution root. That file records the `TurtlePath.Template` package version used to create the service.
