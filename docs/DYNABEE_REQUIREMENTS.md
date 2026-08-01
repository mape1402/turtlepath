# DynaBee Generation Requirements

## Objective

Enable consumer libraries to generate concrete runtime types from declarative descriptors without depending on static factory entry points or custom reflection emit code outside DynaBee.

The generated types should be suitable for dependency injection registration, runtime handler resolution, and extension through alternate generation strategies.

## Current Need

A consumer may have a normalized descriptor that defines:

- The service contract to register.
- The concrete base type to inherit.
- The constructor dependencies to forward to the base type.
- Optional generated members that override base behavior.
- Metadata needed to map generated types back to their source descriptor.

The consumer needs to transform that descriptor into one or more generated CLR types using DynaBee through injectable services.

## Required Capabilities

### DI-First Assembly Creation

DynaBee should support assembly generation through injectable abstractions as the recommended path.

Required services:

- `IDynaBeeAssemblyBuilderFactory` for creating `IBeeAssemblyBuilder`.
- `IAssemblyContextRegistry` for appending generation definitions.
- `IAssemblyContextProvider` for accessing and rebuilding immutable snapshots.
- `IDynaBeeAssemblyCatalog` for resolving generated assemblies by logical name.

The static builder entrypoint may remain as a convenience API, but consumers should not be forced to depend on it.

### Generated Type Planning

DynaBee should provide a clean way for consumers to describe a generated type before emission.

Needed concepts:

- Logical type name.
- Base type.
- Implemented interfaces.
- Constructor definitions.
- Method definitions.
- Property definitions.
- Metadata attached to the generated type.

The generation plan should be testable before building the dynamic assembly.

### Constructor Forwarding

Generated classes commonly need to inherit from a base type that exposes a non-public constructor.

DynaBee should provide a high-level API for forwarding constructor arguments to a selected base constructor.

Desired shape:

```csharp
builder.AddClass("GeneratedHandler", type => type
    .Inherits(baseType)
    .AddConstructor(ctor => ctor
        .WithParameter<IServiceProvider>("serviceProvider")
        .CallsBase(baseConstructor, args => args.Argument("serviceProvider"))));
```

This avoids requiring each consumer to emit `Ldarg_0`, `Ldarg_1`, `Call`, and `Ret` manually.

### Method Overrides

DynaBee should support overriding virtual or abstract methods from base classes.

Required behavior:

- Select a base `MethodInfo`.
- Emit an overriding method with the correct signature.
- Call `TypeBuilder.DefineMethodOverride` internally.
- Preserve access level compatibility, including protected overrides.
- Fail early with a useful message when the selected method cannot be overridden.

Desired shape:

```csharp
builder.AddClass("GeneratedHandler", type => type
    .Inherits(baseType)
    .OverrideMethod(baseMethod, method => method
        .Emits(il => { /* method body */ })));
```

This should not generate `newslot` methods when an override is requested.

### Property Overrides

DynaBee should support overriding virtual or abstract properties from base classes.

Required behavior:

- Select a base `PropertyInfo`.
- Override getter and/or setter.
- Support protected virtual properties.
- Support constants, lambdas, expressions, and IL bodies where possible.

Desired shape:

```csharp
builder.AddClass("GeneratedQuery", type => type
    .Inherits(baseType)
    .OverrideProperty(baseProperty, property => property
        .Getter(get => get.ReturnsConstant("Name"))));
```

### Metadata And Descriptor Correlation

Consumers need to associate generated CLR types with source descriptors.

DynaBee should allow metadata at:

- Assembly level.
- Type level.
- Constructor level.
- Method level.
- Property level.

Metadata must be readable from `IAssemblyContext` and `ITypeContext` after build.

Example:

```csharp
builder.AddClass("GeneratedHandler", type => type
    .WithMetadata("descriptorKey", descriptorKey));
```

### Selective DI Registration

DynaBee already supports generated type registration in DI. The needed refinement is explicit service registration mapping for generated classes that inherit base classes instead of implementing the desired service interface directly.

Required behavior:

- Register generated concrete type.
- Optionally skip concrete type registration.
- Register generated type as selected interfaces.
- Register generated type as selected base/service types.
- Support caller-provided `ServiceDescriptor` projection.

Desired shape:

```csharp
services.AddDynaBee(context, options => options
    .Register(type => type
        .As(serviceType)
        .WithLifetime(ServiceLifetime.Scoped)
        .SkipConcrete()));
```

### Rebuildable Snapshots

When descriptors change, consumers should be able to append generation definitions and rebuild the assembly snapshot through `IAssemblyContextProvider`.

Required behavior:

- Registry revision should change when generation definitions are appended.
- Provider should rebuild when revision changes.
- Consumers should be able to explicitly call `Rebuild`.
- Rebuilds should not depend on static global state.

### Diagnostics

DynaBee should expose diagnostics that explain what was generated and why generation failed.

Useful diagnostics:

- Generated assembly name and revision.
- Generated type names.
- Base type for each generated type.
- Constructors emitted.
- Interfaces/service mappings.
- Methods/properties emitted or overridden.
- Metadata keys.
- Invalid override/property/constructor errors.

## Non-Goals

- DynaBee does not need to understand any specific application framework.
- DynaBee does not need to own business descriptors from consumer libraries.
- DynaBee does not need to provide application-level handler abstractions.
- DynaBee should remain a generic dynamic type generation library.

## Acceptance Criteria

- Consumers can generate concrete classes that inherit closed generic base types using DI-provided DynaBee services.
- Consumers can override abstract or virtual methods and properties without writing raw `Reflection.Emit` plumbing.
- Consumers can attach and read metadata from generated types.
- Consumers can register generated types against arbitrary service types in `IServiceCollection`.
- Consumers can replace the generation strategy behind their own interfaces without depending on DynaBee static APIs.
- Existing static builder APIs continue to work as convenience APIs.

