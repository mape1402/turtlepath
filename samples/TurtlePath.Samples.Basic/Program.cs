using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Pelican.Mediator;
using TurtlePath.Domain.Identifier;
using TurtlePath.EntityFrameworkCore;
using TurtlePath.Mapping;
using TurtlePath.Queries;
using TurtlePath.Validation;
using TurtlePath.Samples.Basic.Application.Queries;
using TurtlePath.Samples.Basic.Application.Requests;
using TurtlePath.Samples.Basic.Infrastructure;
using TurtlePath.Samples.Basic.Infrastructure.Adapters;
using TurtlePath.Samples.Basic.Infrastructure.Persistence;

var sampleAssembly = typeof(Program).Assembly;
var sqliteConnection = new SqliteConnection("Data Source=:memory:");
await sqliteConnection.OpenAsync();

var services = new ServiceCollection();

services.AddOptions();
services.AddSingleton(sqliteConnection);
services.AddSingleton<SampleAuditLog>();
services.AddScoped<IMapperAdapter, SampleMapperAdapter>();
services.AddScoped<IValidatorAdapter, SampleValidatorAdapter>();
services.AddPelican(sampleAssembly);
services.AddDbContext<CommerceDbContext>((provider, options) =>
    options.UseSqlite(provider.GetRequiredService<SqliteConnection>()));

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

var persistedCustomers = await dbContext.Customers.CountAsync();
var persistedOrders = await dbContext.TenantOrders.CountAsync();
var persistedInvoices = await dbContext.LegacyInvoices.CountAsync();
var persistedShipments = await dbContext.LegacyShipments.CountAsync();

Console.WriteLine("TurtlePath commerce sample");
Console.WriteLine($"Default customer CId: {ada.Id}");
Console.WriteLine($"Generated customer CId from factory: {idFactory.New()}");
Console.WriteLine($"Updated customer: {updatedAda.Name} <{updatedAda.Email}>");
Console.WriteLine($"Patched customer email: {customerById.Email}");
Console.WriteLine($"Order CId: {order.Id}");
Console.WriteLine($"Legacy invoice CId: {order.LegacyInvoiceId}");
Console.WriteLine($"Deleted resource: {deletedOrder.Resource} {deletedOrder.Id}");
Console.WriteLine($"Generic int-key shipment: {legacyShipment.Id} {legacyShipment.Carrier} {legacyShipment.TrackingNumber}");
Console.WriteLine($"Filtered customers: {matchingCustomers.Count}");
Console.WriteLine($"Paged customers: page={customerPage.CurrentPage}/{customerPage.PageCount}, rows={customerPage.RowCount}, first={customerPage.Results.First().Name}");
Console.WriteLine($"Persisted rows: customers={persistedCustomers}, orders={persistedOrders}, invoices={persistedInvoices}, shipments={persistedShipments}");
Console.WriteLine($"Audit entries: {auditLog.Entries.Count}");

foreach (var entry in auditLog.Entries)
    Console.WriteLine($"- {entry}");

await sqliteConnection.DisposeAsync();
