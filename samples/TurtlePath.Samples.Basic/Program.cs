using Crabalidator.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using OctoMap;
using Pelican.Mediator;
using TurtlePath.Automations;
using TurtlePath.Crabalidator;
using TurtlePath.Domain.Identifier;
using TurtlePath.EntityFrameworkCore;
using TurtlePath.OctoMap;
using TurtlePath.Queries;
using TurtlePath.Samples.Basic.Application.Queries;
using TurtlePath.Samples.Basic.Application.Requests;
using TurtlePath.Samples.Basic.Infrastructure;
using TurtlePath.Samples.Basic.Infrastructure.Persistence;

var sampleAssembly = typeof(Program).Assembly;
var sqliteConnection = new SqliteConnection("Data Source=:memory:");
await sqliteConnection.OpenAsync();

var services = new ServiceCollection();

services.AddOptions();
services.AddSingleton(sqliteConnection);
services.AddSingleton<SampleAuditLog>();
services.AddPelican(sampleAssembly);
services.AddCrabalidator(sampleAssembly);
services.AddOctoMap(registration =>
{
    registration.Options.EnableRuntimeImplicitMaps = false;
    registration.Options.DuplicateMapPolicy = DuplicateMapPolicy.Throw;
    registration.AddMaps(sampleAssembly);
});
services.AddDbContext<CommerceDbContext>((provider, options) =>
    options.UseSqlite(provider.GetRequiredService<SqliteConnection>()));
services.AddScoped<LegacyInvoiceIdFactory>();

services
    .AddTurtlePath(sampleAssembly)
    .UseCId<Guid, string>(config =>
    {
        config.DefaultFactory = () => CId.From(Guid.NewGuid());
        config.DbType = "uniqueidentifier";
        config.ConvertToDb = id => id.ToString();
        config.ConvertFromDb = value => CId.From(Guid.Parse(value));
        config.JsonConverter = value => CId.From(Guid.Parse(value));
        config.NullableJsonConverter = value => string.IsNullOrWhiteSpace(value) ? null : CId.From(Guid.Parse(value));
        config.ParseFunction = value => CId.From(Guid.Parse(value));
        config.ToByteArrayFunction = value => value.ToByteArray();
    })
    .UseCIdProfiles(sampleAssembly)
    .UseAutomations(sampleAssembly)
    .UseOctoMap()
    .UseCrabalidator()
    .UseSieve()
    .UseEntityFrameworkCore<CommerceDbContext>(options => options with
    {
        ConfigurationAssemblies = [sampleAssembly]
    });

using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();

var scopedProvider = scope.ServiceProvider;
var dbContext = scopedProvider.GetRequiredService<CommerceDbContext>();
await dbContext.Database.EnsureDeletedAsync();
await dbContext.Database.EnsureCreatedAsync();

var idFactory = scopedProvider.GetRequiredService<ICIdFactory>();
var auditLog = scopedProvider.GetRequiredService<SampleAuditLog>();
var mediator = scopedProvider.GetRequiredService<IMediator>();

var ada = await mediator.Send(new CreateCustomerRequest("Ada Lovelace", "ADA@EXAMPLE.COM"));
var grace = await mediator.Send(new CreateCustomerRequest("Grace Hopper", "GRACE@EXAMPLE.COM"));

var updatedAda = await mediator.Send(new UpdateCustomerRequest
{
    Id = ada.Id,
    Name = "Ada Byron",
    Email = "ADA.BYRON@EXAMPLE.COM"
});

var patchedAda = await mediator.Send(new PatchCustomerEmailRequest
{
    Id = updatedAda.Id,
    Email = "ada@turtlepath.dev"
});

var orderRequest = new CreateTenantOrderRequest(
    patchedAda.Id,
    189.95m);
var order = await mediator.Send(orderRequest);

var deletedOrder = await mediator.Send(new DeleteTenantOrderRequest
{
    Id = order.Id
});

var shipment = await mediator.Send(new CreateLegacyShipmentRequest(
    42,
    "LegacyCarrier",
    "TRACK-00042"));

var customerById = await mediator.Send(new GetCustomerByIdQuery(patchedAda.Id));
var matchingCustomers = (await mediator.Send(new GetCustomersQuery
{
    Search = "ada",
    Filters = "Email@=*turtlepath.dev",
    Sorts = "Name"
})).ToList();
var customerPage = await mediator.Send(new GetCustomersPageQuery(new PagedSettings
{
    PageNumber = 1,
    PageSize = 1,
    Sorts = "Name"
}));
var legacyShipment = await mediator.Send(new GetLegacyShipmentByIdQuery(shipment.Id));
var invoice = await mediator.Send(new CreateLegacyInvoiceRequest(
    patchedAda.Id,
    415.50m));
var updatedInvoice = await mediator.Send(new UpdateLegacyInvoiceRequest
{
    Id = invoice.Id,
    CustomerId = invoice.CustomerId,
    Amount = 512.75m
});
var invoiceById = await mediator.Send(new GetLegacyInvoiceByIdQuery(updatedInvoice.Id));
var catalogItem = await mediator.Send(new CreateCatalogItemRequest(
    "TP-001",
    "TurtlePath Field Guide",
    29.95m));
var updatedCatalogItem = await mediator.Send(new UpdateCatalogItemRequest
{
    Id = catalogItem.Id,
    Sku = "TP-001",
    Name = "TurtlePath Field Guide - Revised",
    Price = 34.95m
});
var catalogItemById = await mediator.Send(new GetCatalogItemByIdQuery(updatedCatalogItem.Id));
var deletedCatalogItem = await mediator.Send(new DeleteCatalogItemRequest
{
    Id = catalogItemById.Id
});

var persistedCustomers = await dbContext.Customers.CountAsync();
var persistedOrders = await dbContext.TenantOrders.CountAsync();
var persistedInvoices = await dbContext.LegacyInvoices.CountAsync();
var persistedShipments = await dbContext.LegacyShipments.CountAsync();
var persistedCatalogItems = await dbContext.CatalogItems.CountAsync();

Console.WriteLine("TurtlePath commerce sample");
Console.WriteLine($"Default customer CId: {ada.Id}");
Console.WriteLine($"Generated customer CId from factory: {idFactory.New()}");
Console.WriteLine($"Updated customer: {updatedAda.Name} <{updatedAda.Email}>");
Console.WriteLine($"Patched customer email: {customerById.Email}");
Console.WriteLine($"Order CId: {order.Id}");
Console.WriteLine($"Legacy invoice CId: {order.LegacyInvoiceId}");
Console.WriteLine($"Deleted resource: {deletedOrder.Resource} {deletedOrder.Id}");
Console.WriteLine($"Automated invoice flow: {invoiceById.Id} customer={invoiceById.CustomerId} amount={invoiceById.Amount:C}");
Console.WriteLine($"Generic int-key shipment: {legacyShipment.Id} {legacyShipment.Carrier} {legacyShipment.TrackingNumber}");
Console.WriteLine($"Attribute catalog item: {catalogItemById.Sku} {catalogItemById.Name} {catalogItemById.Price:C}");
Console.WriteLine($"Deleted attribute resource: {deletedCatalogItem.Resource} {deletedCatalogItem.Id}");
Console.WriteLine($"Filtered customers: {matchingCustomers.Count}");
Console.WriteLine($"Paged customers: page={customerPage.CurrentPage}/{customerPage.PageCount}, rows={customerPage.RowCount}, first={customerPage.Results.First().Name}");
Console.WriteLine($"Persisted rows: customers={persistedCustomers}, orders={persistedOrders}, invoices={persistedInvoices}, shipments={persistedShipments}, catalogItems={persistedCatalogItems}");
Console.WriteLine($"Audit entries: {auditLog.Entries.Count}");

foreach (var entry in auditLog.Entries)
    Console.WriteLine($"- {entry}");

await sqliteConnection.DisposeAsync();
