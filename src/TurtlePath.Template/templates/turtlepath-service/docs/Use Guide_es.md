# Guía De Uso De DTemplate

Guía de uso para servicios ASP.NET Core construidos con TurtlePath.

## Índice

- [Stack](#stack)
- [Primeros Pasos](#primeros-pasos)
- [Organización Por Feature](#organización-por-feature)
- [Convenciones De Nomenclatura](#convenciones-de-nomenclatura)
- [Automations](#automations)
- [Handlers Custom](#handlers-custom)
- [Hooks](#hooks)
- [Mapeo Y Validación](#mapeo-y-validación)
- [Testing](#testing)
- [Transacciones](#transacciones)
- [Checklist De Migración](#checklist-de-migración)
- [Notas De Actualización Del Template](#notas-de-actualización-del-template)

## Stack

- `TurtlePath` para handlers, hooks, requests, responses y excepciones de aplicación.
- `TurtlePath.Domain` para `CId`, `BaseEntity` e `IEntity<TKey>`.
- `TurtlePath.EntityFrameworkCore` para `BaseDbContext`, `IDbContext`, adapters de almacenamiento y convenciones de EF.
- `TurtlePath.OctoMap` para mapeo.
- `TurtlePath.Crabalidator` para validación.
- `TurtlePath.DataScorpio` para filtros, ordenamiento y paginación.
- `TurtlePath.Analyzers` en Domain y Business para detectar comparaciones y asignaciones inseguras de `CId`.
- `Pigeon.Messaging` con Azure Service Bus como broker predeterminado y Outbox con EF Core habilitado por defecto.
- `Spider.Pipelines` para boundaries de mensajería y el boundary transaccional.

## Primeros Pasos

El template mantiene la infraestructura compartida en paquetes TurtlePath y organiza el código del servicio por feature.

### Registro Del Stack

La capa API registra el assembly de Business, TurtlePath, adapters, configuración de identificadores, EF Core, Pigeon y Spider:

```csharp
services.AddCrabalidator(typeof(Constants).Assembly);

services.AddOctoMap(registration =>
{
    registration.Options.EnableRuntimeImplicitMaps = true;
    registration.Options.DuplicateMapPolicy = DuplicateMapPolicy.Throw;
    registration.AddMaps(typeof(Constants).Assembly);
});

services.AddScoped<IMapperAdapter, OctoMapAdapter>();
services.AddScoped<IValidatorAdapter, CrabalidatorAdapter>();

services.AddTurtlePath(typeof(Constants).Assembly)
    .UseOctoMap()
    .UseCrabalidator()
    .UseDataScorpio(profiles => profiles.FromAssembly(typeof(Constants).Assembly))
    .UseCId<Ulid, string>(config =>
    {
        config.DefaultFactory = () => CId.From(Ulid.NewUlid());
        config.ConvertToDb = id => id.ToString();
        config.ConvertFromDb = value => CId.From(Ulid.Parse(value));
        config.JsonConverter = value => string.IsNullOrEmpty(value) ? CId.From(Ulid.Empty) : CId.From(Ulid.Parse(value));
        config.NullableJsonConverter = value => string.IsNullOrEmpty(value) ? null : CId.From(Ulid.Parse(value));
        config.ParseFunction = value => CId.From(Ulid.Parse(value));
    })
    .UseEntityFrameworkCore<AppDbContext>();
```

Pigeon usa Azure Service Bus por defecto:

```csharp
services.AddPigeon(configuration, builder =>
{
    // builder.ScanConsumersFromAssemblies(typeof(Program).Assembly);
    builder.ConfigurePublishing(publishing =>
    {
        publishing.AmbientTransactionBehavior = AmbientTransactionPublishBehavior.SuppressTransaction;
    });

    builder.UseAzureServiceBus();
    builder.UseEntityFrameworkOutbox<AppDbContext>(outbox =>
    {
        outbox.Enabled = true;
        outbox.SchemaMode = OutboxSchemaMode.AutoCreate;
    });
});
```

La configuracion de Outbox se puede ajustar desde `Pigeon:Outbox`. El template usa almacenamiento EF Core por defecto, creacion automatica de schema, despacho inmediato despues del commit, reintentos y limpieza periodica. Cuando se publica directo al broker dentro de una transaccion ambiental sin Outbox, Pigeon suprime explicitamente el transaction scope.

### Flujo Manual Mínimo

Entidad:

```csharp
using TurtlePath.Domain.Contracts;

public sealed class Customer : BaseEntity
{
    public string Name { get; set; }
}
```

Request y response:

```csharp
using Pelican.Mediator;
using TurtlePath.Models.Responses;

public sealed class CreateCustomerRequest : IRequest<CustomerResponse>
{
    public string Name { get; set; }
}

public sealed class CustomerResponse : BaseResponse
{
    public string Name { get; set; }
}
```

Handler:

```csharp
using TurtlePath.Commands;

public sealed class CreateCustomerCommandHandler
    : CreateCommandHandler<CreateCustomerRequest, CustomerResponse, Customer>
{
    public CreateCustomerCommandHandler(IServiceProvider services) : base(services)
    {
    }
}
```

Controller:

```csharp
[HttpPost]
public Task<CustomerResponse> Create(CreateCustomerRequest request, CancellationToken cancellationToken)
{
    return Mediator.Send(request, cancellationToken);
}
```

Usa handlers manuales cuando el flujo tiene comportamiento de negocio custom que debe ser explícito en código. Usa automations para CRUD happy paths estándar.

## Organización Por Feature

El código de Business debe organizarse por feature, no por carpetas técnicas globales.

```text
Feature/
  Commands/
  Queries/
  Validators/
  Mappings/
  Hooks/
  Automations/
  Querying/
  Models/
    Requests/
    Responses/
  Services/
```

`Feature` es un placeholder. Sustitúyelo por la capacidad real de negocio al crear código.

Ejemplos:

```text
Customers/
  Commands/
  Queries/
  Validators/
  Mappings/
  Hooks/
  Automations/
  Querying/
  Models/
    Requests/
    Responses/
  Services/

Invoices/
  Commands/
  Queries/
  Validators/
  Mappings/
  Hooks/
  Automations/
  Querying/
  Models/
    Requests/
    Responses/
  Services/

Orders/
  Commands/
  Queries/
  Validators/
  Mappings/
  Hooks/
  Automations/
  Querying/
  Models/
    Requests/
    Responses/
  Services/
```

No crees un contenedor genérico `Features`. Cada feature vive directamente debajo del proyecto Business.

Guías:

- Coloca comandos que mutan estado en `Commands`.
- Coloca requests de lectura en `Queries`.
- Coloca DTOs de request/response en `Models/Requests` y `Models/Responses`.
- Coloca validators de Crabalidator en `Validators`.
- Coloca maps/profiles de OctoMap en `Mappings`.
- Coloca hooks de TurtlePath en `Hooks`.
- Coloca automation profiles o attributes de TurtlePath en `Automations`.
- Coloca perfiles `QueryProfile<TEntity>` de DataScorpio en `Querying`.
- Coloca colaboradores propios del feature en `Services`.
- Cuando un feature necesita una integración externa, colócala dentro de `Services` usando una carpeta específica del servicio, como `Services/SAT`.

Los servicios compartidos por varios features deben vivir en la raíz de Business y también agruparse por servicio:

```text
Services/
  Audit/
```

Esto deja limpio el camino de extracción futura si el servicio se convierte en una librería compartida.

## Convenciones De Nomenclatura

### Requests

Los mensajes de mutación son conceptualmente commands, pero sus clases deben conservar el sufijo `Request`:

- `CreateCustomerRequest`
- `UpdateInvoiceRequest`
- `ChangeOrderStatusRequest`

### Responses

Los responses representan la salida de cualquier handler:

- `CustomerResponse`
- `InvoiceResponse`
- `ChangeOrderStatusResponse`

### Command Handlers

Los command handlers expresan la acción y terminan con `CommandHandler`:

- `CreateCustomerCommandHandler`
- `UpdateInvoiceCommandHandler`
- `ChangeOrderStatusCommandHandler`

Ejemplo:

```csharp
public sealed class ChangeOrderStatusCommandHandler
    : CreateCommandHandler<ChangeOrderStatusRequest, ChangeOrderStatusResponse, Order>
{
    public ChangeOrderStatusCommandHandler(IServiceProvider services) : base(services)
    {
    }
}
```

### Queries

Los mensajes query y sus handlers usan terminología `Query`:

- `GetCustomerByIdQuery`
- `GetCustomerByIdQueryHandler`
- `GetPagedInvoicesQuery`
- `GetPagedInvoicesQueryHandler`

Para flujos pequeños, deja el handler anidado dentro del query:

```csharp
public sealed class GetCustomerByIdQuery : IRequest<CustomerResponse>
{
    public CId Id { get; set; }

    public sealed class GetCustomerByIdQueryHandler
        : QueryByIdHandler<GetCustomerByIdQuery, CustomerResponse, Customer>
    {
        public GetCustomerByIdQueryHandler(IServiceProvider services) : base(services)
        {
        }
    }
}
```

Cuando un flujo se genera con automations, omite command handlers y query handlers manuales.

### Validators

Los validators usan el nombre del request más `Validator`:

- `CreateCustomerRequestValidator`
- `UpdateInvoiceRequestValidator`
- `ChangeOrderStatusRequestValidator`

### Mappings

Los mapping profiles usan el nombre del feature o aggregate más `MappingProfile`:

- `CustomerMappingProfile`
- `InvoiceMappingProfile`
- `OrderMappingProfile`

### Automations

Los automation profiles usan el nombre del feature o aggregate más `AutomationProfile`:

- `CustomerAutomationProfile`
- `InvoiceAutomationProfile`
- `OrderAutomationProfile`

### Hooks

Los hooks deben describir la acción y el punto donde se ejecutan:

- `AssignCustomerNumberBeforeSaveHook`
- `PublishInvoiceCanceledAfterSaveHook`
- `NormalizeOrderStatusBeforeValidationHook`

### Services

Los servicios propios del feature viven dentro del feature, bajo una carpeta específica del servicio.

```text
Customers/
  Services/
    SAT/
      ISatService.cs
      SatService.cs
```

Ejemplo de método:

```csharp
public interface ISatService
{
    Task<bool> ValidateCustomerRfc(string rfc, CancellationToken cancellationToken);
}
```

Los servicios compartidos viven en la raíz de Business, también agrupados por servicio:

```text
Services/
  Audit/
    IAuditService.cs
    AuditService.cs
```

### Controllers

Los controllers usan nombres de recursos en plural:

- `CustomersController`
- `InvoicesController`
- `OrdersController`

Usa rutas RESTful para CRUD:

- `[POST] customers` crea un recurso.
- `[PUT] customers/{id}` actualiza un recurso.
- `[DELETE] customers/{id}` elimina un recurso.
- `[GET] customers` lee la colección, normalmente paginada.
- `[GET] customers/{id}` lee un recurso por id.

Los filtros adicionales de consultas paginadas se agregan como query filters y se resuelven dinámicamente con DataScorpio.

Usa `POST` para acciones fuera de CRUD:

- `[POST] customers/{id}/deactivate`
- `[POST] invoices/{id}/cancel`

Usa rutas anidadas para subrecursos:

- `[GET] customers/{id}/orders`
- `[DELETE] orders/{id}/details/{detailId}`

### Hub Consumers

Los hub consumers también usan el nombre del recurso en plural:

- `CustomersHubConsumer`
- `InvoicesHubConsumer`

### Entity Configurations

Las configuraciones de entidad usan el nombre de la entidad más `Configuration`:

- `CustomerConfiguration`
- `InvoiceConfiguration`
- `OrderConfiguration`

## Automations

Usa `TurtlePath.Automations` cuando el feature sigue el happy path estándar y no necesita un handler custom.

### Fluent Profile

```csharp
using TurtlePath.Automations.Profiles;

public sealed class CustomerAutomationProfile : AutomationProfile
{
    public override void Configure(IAutomationProfileBuilder builder)
    {
        builder.For<Customer>()
            .ToCreate<CreateCustomerRequest, CustomerResponse>()
            .ToUpdate<UpdateCustomerRequest, CustomerResponse>()
            .ToDelete<DeleteCustomerRequest>()
            .ToQueryById<GetCustomerByIdQuery, CustomerResponse>()
            .ToQueryPaged<GetPagedCustomersQuery, CustomerResponse>();
    }
}
```

Registra los automation profiles desde la composición de API cuando el servicio empiece a usar automations.

### Attributes

Los attributes son útiles para features pequeños donde el request puede declarar la intención de automation.

```csharp
[AutomateCreate(typeof(Customer), typeof(CustomerResponse))]
public sealed class CreateCustomerRequest : IRequest<CustomerResponse>
{
    public string Name { get; set; }
}
```

### Customización Sin Handlers

Prefiere hooks cuando el flujo predeterminado sigue siendo correcto pero necesita un paso de negocio. Revisa [Hooks](#hooks) para la lista completa de command hooks y query hooks.

Usa un handler custom cuando el flujo cambia bastante, la operación toca varios aggregates o el comportamiento sería difícil de entender solo con hooks.

## Handlers Custom

Usa handlers custom cuando el happy path de TurtlePath no es suficiente.

Los handlers recomendados usan `BaseEntity` y `CId`:

```csharp
public sealed class CreateCustomerCommandHandler
    : CreateCommandHandler<CreateCustomerRequest, CustomerResponse, Customer>
{
    public CreateCustomerCommandHandler(IServiceProvider services) : base(services)
    {
    }
}
```

Para contratos legacy o llaves custom, usa los handlers genéricos:

```csharp
public sealed class CreateLegacyCustomerCommandHandler
    : GenericCreateCommandHandler<CreateLegacyCustomerRequest, LegacyCustomerResponse, LegacyCustomer, int>
{
    public CreateLegacyCustomerCommandHandler(IServiceProvider services) : base(services)
    {
    }
}
```

Los handlers conservan métodos virtuales para customización puntual. Prefiere sobreescribir un método cuando el cambio pertenece solo a ese handler. Prefiere un hook cuando el comportamiento será reutilizable entre handlers o automations.

### Métodos Virtuales De Command Handlers

Create handlers:

- `ValidateRequest`: habilita o deshabilita validación del request. Por defecto es `true`.
- `UseProjectionFromStorage`: mapea el response desde una proyección fresca de storage después de guardar. Solo aplica a handlers con response. Por defecto es `false`.
- `ValidateAsync(request, cancellationToken)`: valida el request.
- `MapToEntityAsync(request, cancellationToken)`: crea la entidad desde el request.
- `SaveEntityAsync(request, entity, cancellationToken)`: persiste la nueva entidad.
- `MapToResponseAsync(request, entity, cancellationToken)`: construye el response. Solo aplica a handlers con response.

Update handlers:

- `ValidateRequest`: habilita o deshabilita validación del request. Por defecto es `true`.
- `UseProjectionFromStorage`: mapea el response desde una proyección fresca de storage después de guardar. Solo aplica a handlers con response. Por defecto es `false`.
- `GetEntityAsync(request, cancellationToken)`: carga la entidad a actualizar.
- `ValidateAsync(request, entity, cancellationToken)`: valida el request con la entidad cargada.
- `MapEntityAsync(request, entity, cancellationToken)`: mapea valores del request sobre la entidad.
- `UpdateEntityAsync(request, entity, cancellationToken)`: guarda la entidad actualizada.
- `MapToResponseAsync(request, entity, cancellationToken)`: construye el response. Solo aplica a handlers con response.

Delete handlers:

- `ValidateRequest`: habilita o deshabilita validación del request. Por defecto es `false`.
- `GetEntityAsync(request, cancellationToken)`: carga la entidad a eliminar.
- `ValidateAsync(request, entity, cancellationToken)`: valida si el delete está permitido.
- `DeleteEntityAsync(entity, cancellationToken)`: elimina la entidad.
- `BuildResponseAsync(request, entity, cancellationToken)`: construye el response. Solo aplica a handlers con response.

Patch handlers:

- `ValidateRequest`: habilita o deshabilita validación del request. Por defecto es `false`.
- `GetEntityAsync(request, cancellationToken)`: carga la entidad a aplicar patch.
- `ValidateAsync(request, entity, cancellationToken)`: valida si el patch está permitido.
- `PatchEntityAsync(request, entity, cancellationToken)`: aplica la acción de patch sobre la entidad.
- `UpdateEntityAsync(request, entity, cancellationToken)`: guarda la entidad modificada.
- `BuildResponseAsync(request, entity, cancellationToken)`: construye el response. Solo aplica a handlers con response.

Ejemplo: deshabilitar validación para un comando interno idempotente.

```csharp
public sealed class TouchCustomerCommandHandler
    : UpdateCommandHandler<TouchCustomerRequest, Customer>
{
    public TouchCustomerCommandHandler(IServiceProvider services) : base(services)
    {
    }

    protected override bool ValidateRequest => false;
}
```

Ejemplo: agregar una regla de lookup específica del feature.

```csharp
public sealed class CancelInvoiceCommandHandler
    : UpdateCommandHandler<CancelInvoiceRequest, InvoiceResponse, Invoice>
{
    public CancelInvoiceCommandHandler(IServiceProvider services) : base(services)
    {
    }

    protected override async Task<Invoice> GetEntityAsync(CancelInvoiceRequest request, CancellationToken cancellationToken)
    {
        var invoice = await StorageReaderAdapter
            .For<Invoice>()
            .Where(x => x.Id == request.Id && !x.IsCanceled)
            .FirstOrDefaultAsync(cancellationToken);

        return invoice ?? throw new NotFoundException(nameof(Invoice), request.Id.ToString());
    }
}
```

### Métodos Virtuales De Query Handlers

Get one y get by id handlers:

- `Handle(query, cancellationToken)`: se puede sobreescribir cuando debe cambiar todo el flujo de query.
- `GetFilterExpression(query)`: define cómo se localiza la entidad.

Get many handlers:

- `Handle(query, cancellationToken)`: se puede sobreescribir cuando debe cambiar todo el flujo de query.
- `GetFilterExpression(query)`: agrega filtros tipados antes de los filtros string de DataScorpio.
- `GetSortingExpression(query)`: agrega ordenamiento tipado antes del ordenamiento string de DataScorpio.

Get paged handlers:

- `DefaultSorts`: define el ordenamiento compatible con DataScorpio cuando el request no especifica sorts.
- `GetFiltersExpression(query)`: agrega filtros tipados antes de los filtros string de DataScorpio.
- `GetSortingExpression(query)`: agrega ordenamiento tipado antes del ordenamiento string de DataScorpio.

Ejemplo: forzar una lista de invoices por customer, manteniendo filtros de DataScorpio.

```csharp
public sealed class GetPagedInvoicesQueryHandler
    : GetPagedInfoQueryHandler<GetPagedInvoicesQuery, Invoice, InvoiceResponse>
{
    public GetPagedInvoicesQueryHandler(IServiceProvider services) : base(services)
    {
    }

    protected override string DefaultSorts => "-CreatedAt";

    protected override Expression<Func<Invoice, bool>> GetFiltersExpression(GetPagedInvoicesQuery query)
    {
        return invoice => invoice.CustomerId == query.CustomerId;
    }
}
```

## Hooks

Los hooks permiten customizar el path estándar sin reemplazar el handler.

Usa hooks cuando el flujo predeterminado es correcto y solo necesitas agregar comportamiento en un punto conocido. Los hooks se descubren con dependency injection, se ejecutan en orden de registro y pueden implementar `IOrderedHook` cuando varios hooks apuntan al mismo stage.

Los command hooks usan `CommandHookContext<TRequest, TEntity>` o `CommandHookContext<TRequest, TEntity, TResponse>`. El contexto expone `Request`, `Entity`, `Response` cuando está disponible, y almacenamiento tipado key/value para compartir datos entre hooks en la misma ejecución del handler.

Command hooks disponibles:

- `IBeforeValidationHook<TRequest, TEntity>`: normaliza datos del request o carga contexto antes de validar.
- `IAfterValidationHook<TRequest, TEntity>`: ejecuta lógica que depende de un request válido.
- `IBeforeGetEntityHook<TRequest, TEntity>`: prepara datos antes de que update, delete o patch carguen la entidad.
- `IAfterGetEntityHook<TRequest, TEntity>`: inspecciona o enriquece la entidad cargada antes de validación o mutación.
- `IBeforeMapHook<TRequest, TEntity>`: normaliza datos antes del mapeo de create o update.
- `IAfterMapHook<TRequest, TEntity>`: fuerza valores derivados de la entidad después del mapeo.
- `IBeforePatchHook<TRequest, TEntity>`: valida precondiciones de patch.
- `IAfterPatchHook<TRequest, TEntity>`: fuerza valores derivados después de aplicar patch.
- `IBeforeSaveHook<TRequest, TEntity>`: asigna campos de auditoría, números de negocio o prepara efectos antes de persistir.
- `IAfterSaveHook<TRequest, TEntity>`: publica mensajes, escribe auditoría o dispara trabajo async después de persistir.
- `IBeforeDeleteHook<TRequest, TEntity>`: bloquea eliminación o prepara limpieza relacionada.
- `IAfterDeleteHook<TRequest, TEntity>`: publica eventos de eliminación o escribe auditoría.
- `IBeforeResponseHook<TRequest, TEntity, TResponse>`: enriquece datos antes de construir el response.
- `IAfterResponseHook<TRequest, TEntity, TResponse>`: ajusta o enriquece el response antes de devolverlo.

Query hooks disponibles:

- `IBeforeQueryHook<TQuery, TResult>`: aplica contexto de lectura, captura datos de telemetría o valida precondiciones del query.
- `IAfterQueryHook<TQuery, TResult>`: enriquece resultados, captura métricas o escribe auditoría de lectura.

Ejemplo: asignar un número de negocio antes de guardar un customer.

```csharp
public sealed class AssignCustomerNumberBeforeSaveHook
    : IBeforeSaveHook<CreateCustomerRequest, Customer>
{
    private readonly ICustomerNumberService customerNumberService;

    public AssignCustomerNumberBeforeSaveHook(ICustomerNumberService customerNumberService)
    {
        this.customerNumberService = customerNumberService;
    }

    public async ValueTask BeforeSaveAsync(CommandHookContext<CreateCustomerRequest, Customer> context, CancellationToken cancellationToken)
    {
        context.Entity.CustomerNumber = await customerNumberService.NextAsync(cancellationToken);
    }
}
```

Ejemplo: publicar un evento de integración después de guardar.

```csharp
public sealed class PublishCustomerCreatedAfterSaveHook
    : IAfterSaveHook<CreateCustomerRequest, Customer>
{
    private readonly ISpider spider;

    public PublishCustomerCreatedAfterSaveHook(ISpider spider)
    {
        this.spider = spider;
    }

    public async ValueTask AfterSaveAsync(CommandHookContext<CreateCustomerRequest, Customer> context, CancellationToken cancellationToken)
    {
        await spider.Send(new CustomerCreatedEvent(context.Entity.Id), cancellationToken);
    }
}
```

Ejemplo: enriquecer el resultado de un query paginado después de ejecutarse.

```csharp
public sealed class AttachInvoiceSummaryAfterQueryHook
    : IAfterQueryHook<GetPagedInvoicesQuery, PagedResponse<InvoiceResponse>>
{
    public ValueTask AfterQueryAsync(QueryHookContext<GetPagedInvoicesQuery, PagedResponse<InvoiceResponse>> context, CancellationToken cancellationToken)
    {
        foreach (var invoice in context.Result.Results)
            invoice.CanBeCanceled = invoice.Status == InvoiceStatus.Pending;

        return ValueTask.CompletedTask;
    }
}
```

Usa hooks para:

- auditoría y telemetría transversal
- enriquecimiento específico del feature
- normalización del request antes de validar o mapear
- efectos secundarios después de guardar o eliminar
- enriquecimiento de response
- validaciones de negocio que encajan naturalmente en un stage

Evita hooks cuando el flujo principal se vuelve difícil de entender sin abrir muchos archivos. En ese caso, usa un handler custom.

## Filtros Con DataScorpio

DataScorpio es el motor predeterminado de filtros y ordenamiento para queries paginadas de TurtlePath en este template. Mantén los query profiles dentro del feature dueño de la entidad:

`	ext
Customers/
  Querying/
    CustomerQueryProfile.cs
`

Ejemplo de profile:

`csharp
using DataScorpio.Profiles;

public sealed class CustomerQueryProfile : QueryProfile<Customer>
{
    public override void Configure(IQueryProfileBuilder<Customer> builder)
    {
        builder
            .AllowFilter(customer => customer.Name)
            .AllowFilter(customer => customer.Email)
            .AllowSort(customer => customer.Name)
            .AllowSort(customer => customer.CreatedAt)
            .AllowSearch(customer => customer.Name)
            .AllowSearch(customer => customer.Email);
    }
}
`

Los profiles se descubren desde la composición de API:

`csharp
services.AddTurtlePath(typeof(Constants).Assembly)
    .UseDataScorpio(profiles => profiles.FromAssembly(typeof(Constants).Assembly));
`

Solo los campos declarados en el profile pueden filtrarse, ordenarse o buscarse. Agrega aliases o custom filters en el profile cuando el nombre público del query deba diferir de la propiedad de la entidad o cuando el filtro necesite comportamiento de negocio específico.
## Mapeo Y Validación

El template usa OctoMap y Crabalidator mediante adapters de TurtlePath.

### Mapeo

Registra maps en el assembly de Business. La composición de API escanea ese assembly:

```csharp
services.AddOctoMap(registration =>
{
    registration.Options.EnableRuntimeImplicitMaps = true;
    registration.Options.DuplicateMapPolicy = DuplicateMapPolicy.Throw;
    registration.AddMaps(typeof(Constants).Assembly);
});
```

Mantén los maps cerca del feature que los posee:

```text
Feature/Mappings/
```

### Validación

Registra validators en el assembly de Business. La composición de API escanea ese assembly:

```csharp
services.AddCrabalidator(typeof(Constants).Assembly);
```

Mantén los validators cerca del request del feature:

```text
Feature/Validators/
```

Los handlers y automations de TurtlePath llaman el validator adapter registrado antes de mapear o guardar.

## Testing

El template incluye un proyecto de pruebas listo para unit tests e integration tests de TurtlePath. Usa `TemplateTestHost` desde `tests/DTemplate.Tests/Testing` para que cada prueba solo agregue los maps, validators, seeds o query profiles específicos del feature.

### Unit Test De Handler

Usa el host en memoria para pruebas rápidas de handlers:

```csharp
await using var host = await TemplateTestHost
    .CreateUnitHost()
    .WithMap<CreateCustomerRequest, Customer>(request => new Customer
    {
        Id = request.Id,
        Name = request.Name
    })
    .WithMap<Customer, CustomerResponse>(customer => new CustomerResponse
    {
        Id = customer.Id,
        Name = customer.Name
    })
    .WithValidRequest<CreateCustomerRequest>()
    .BuildAsync();

var handler = new CreateCustomerCommandHandler(host.Services);

var response = await handler.Handle(new CreateCustomerRequest(1, "Ada"));

Assert.Equal("Ada", response.Name);
Assert.True(host.Store<Customer>().Contains(customer => customer.Id == 1));
```

Esto mantiene la prueba enfocada en el comportamiento del handler mientras TurtlePath aporta los dobles de prueba por defecto.

### Integration Test Con SQLite

Usa `TurtlePath.Testing.EntityFrameworkCore` cuando el flujo debe ejecutar mappings de EF Core, queries y persistencia:

```csharp
await using var host = await TemplateTestHost
    .CreateIntegrationHost<AppDbContext>()
    .UsePelican(typeof(CreateCustomerCommandHandler).Assembly)
    .WithMap<CreateCustomerRequest, Customer>(request => new Customer
    {
        Id = request.Id,
        Name = request.Name
    })
    .WithMap<Customer, CustomerResponse>(customer => new CustomerResponse
    {
        Id = customer.Id,
        Name = customer.Name
    })
    .WithValidRequest<CreateCustomerRequest>()
    .BuildAsync();

await host.CreateSchemaAsync<AppDbContext>();

var response = await host.SendAsync(new CreateCustomerRequest(1, "Ada"));
```

Usa integration tests con SQLite también para automations, porque los handlers generados son infraestructura de runtime y el valor está en probar el happy path completo.

### Testing De Adapters Elysium

`TurtlePath.Testing.Integration` compone TurtlePath con los paquetes de testing de Elysium:

- `UsePelicanTesting(...)` registra servicios de testing de Pelican y descubrimiento de handlers.
- `UseOctoMapTesting(...)` registra los maps reales de OctoMap.
- `UseCrabalidatorTesting(...)` registra los validators reales de Crabalidator.
- `UsePigeonTesting(...)` registra el transporte en memoria de messaging.
- `UseSpiderTesting(...)` registra boundaries y trazas de Spider para testing.
- `UseDataScorpioTesting(...)` registra testing de queries DataScorpio.

Ejemplo:

```csharp
await using var host = await TurtlePathTestHost
    .Create()
    .UsePelicanTesting(typeof(Constants).Assembly)
    .UseOctoMapTesting(typeof(Constants).Assembly)
    .UseCrabalidatorTesting(typeof(Constants).Assembly)
    .UseDataScorpioTesting(profiles => profiles.FromAssembly(typeof(Constants).Assembly))
    .BuildAsync();
```

Usa los helpers con delegates para unit tests aislados y los adapters de testing de Elysium cuando la prueba debe validar los maps, validators, filtros, messaging o boundaries reales.

## Transacciones

El template usa un execution boundary de Spider para transacciones ambientales en lugar de un pipeline behavior de Pelican.

Registro:

```csharp
services.Configure<TransactionBoundaryOptions>(configuration.GetSection("TransactionBoundary"));
services.AddSingleton<ITransactionBoundaryRequestFilter>(provider =>
{
    var filter = new TransactionBoundaryRequestFilter(provider.GetRequiredService<IOptions<TransactionBoundaryOptions>>());
    filter.Discover(typeof(Constants).Assembly);

    return filter;
});

services.AddSpider(builder =>
{
    builder.AddExecutionBoundary<TransactionExecutionBoundary>();
});
```

Configuración:

```json
"TransactionBoundary": {
  "Enabled": true,
  "IncludeQueries": false,
  "IsolationLevel": "ReadCommitted",
  "TimeoutSeconds": 30,
  "ExcludedRequestTypes": []
}
```

Por defecto:

- las mutaciones corren dentro de un `TransactionScope`
- los query requests se omiten
- los requests marcados con `[SkipTransactionBoundary]` se omiten
- los tipos listados en `ExcludedRequestTypes` se omiten
- las decisiones del boundary se descubren y cachean por tipo de request

`ExcludedRequestTypes` acepta el nombre completo del tipo o el nombre corto.

```json
"ExcludedRequestTypes": [
  "RebuildSearchIndexCommand",
  "MyService.Features.Health.Commands.PingExternalDependencyCommand"
]
```

## Checklist De Migración

Usa este checklist al migrar un servicio creado con la versión anterior del template.

- Reemplaza usos de `DTemplate.Domain.Identifier` con `TurtlePath.Domain.Identifier`.
- Reemplaza usos de `DTemplate.Domain.Contracts` con `TurtlePath.Domain.Contracts`.
- Reemplaza usos de `DTemplate.Business.Core.Commands` con `TurtlePath.Commands`.
- Reemplaza usos de `DTemplate.Business.Core.Queries` con `TurtlePath.Queries`.
- Reemplaza usos de `DTemplate.Business.Core.Models.Requests` con `TurtlePath.Models.Requests`.
- Reemplaza usos de `DTemplate.Business.Core.Models.Responses` con `TurtlePath.Models.Responses`.
- Reemplaza usos de `DTemplate.Business.Core.Hooks` con `TurtlePath.Hooks`.
- Reemplaza usos de `DTemplate.Business.Core.Exceptions` con `TurtlePath.Exceptions` o `TurtlePath.Validation`.
- Reemplaza usos locales de `IDbContext` con `TurtlePath.EntityFrameworkCore.IDbContext`.
- Haz que el DbContext concreto herede de `TurtlePath.EntityFrameworkCore.BaseDbContext`.
- Elimina handler core local, implementación local de CId, adapters locales de storage, mapper adapter local y validator adapter local.
- Elimina entity configurations que hereden del viejo `BaseEntityConfiguration<TEntity>`.
- Registra dependencias de Business desde la composición de API.
- Registra TurtlePath desde la composición de API.
- Registra OctoMap mediante `TurtlePath.OctoMap`.
- Registra Crabalidator mediante `TurtlePath.Crabalidator`.
- Registra DataScorpio mediante `TurtlePath.DataScorpio`.
- Habilita Pigeon Outbox con EF Core y asegura que el schema de outbox se cree por auto-create, migraciones o el proceso de despliegue del servicio.
- Manten el comportamiento de publicacion transaccional de Pigeon en `SuppressTransaction`, salvo que quieras fallar cuando alguien publique directo al broker dentro de una transaccion.
- Reemplaza el transaction pipeline behavior de Pelican con el transaction boundary de Spider.
- Agrega `TurtlePath.Analyzers` de forma privada en los proyectos Domain y Business.
- Compila y ejecuta las pruebas de composición antes de migrar código de features.

## Notas De Actualización Del Template

El template se actualizó para consumir los paquetes NuGet publicados de TurtlePath en lugar de cargar localmente la infraestructura extraída de handlers, identificadores, persistencia, mapeo y validación.

Cambios clave:

- `DTemplate.Domain` conserva solo entidades y código de dominio propio del servicio.
- `DTemplate.Business` conserva commands, queries, validators, mappings, hooks, automations y services propios del servicio.
- `DTemplate.Persistence` conserva el contexto EF Core concreto y configuraciones EF propias del servicio.
- La infraestructura compartida de handlers e identificadores ahora vive en paquetes TurtlePath.
- El registro de dependencias de Business se movió a la composición de API.
- El comportamiento transaccional cambió de un pipeline behavior de Pelican a un execution boundary de Spider.
- Pigeon usa Azure Service Bus con Outbox EF Core por defecto.

Verificación:

```powershell
dotnet restore DTemplate.sln --verbosity minimal
dotnet build DTemplate.sln --configuration Release --no-restore --verbosity minimal
dotnet test DTemplate.sln --configuration Release --no-build --verbosity minimal
```
