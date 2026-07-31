using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Pelican.Mediator;
using TurtlePath.Domain.Identifier;
using TurtlePath.EntityFrameworkCore;
using TurtlePath.Mapping;
using TurtlePath.Persistence;
using TurtlePath.Validation;
using TurtlePath.Samples.Basic.Application.Requests;
using TurtlePath.Samples.Basic.Infrastructure;
using TurtlePath.Samples.Basic.Infrastructure.Adapters;
using TurtlePath.Samples.Basic.Infrastructure.Persistence;

var sampleAssembly = typeof(Program).Assembly;
var sqliteConnection = new SqliteConnection("Data Source=:memory:");
await sqliteConnection.OpenAsync();

var services = new ServiceCollection();

services.AddSingleton(sqliteConnection);
services.AddSingleton<SampleAuditLog>();
services.AddScoped<IMapperAdapter, SampleMapperAdapter>();
services.AddScoped<IValidatorAdapter, SampleValidatorAdapter>();
services.AddScoped<IStorageWriterAdapter, StorageWriterAdapter>();
services.AddScoped<IStorageReaderAdapter, StorageReaderAdapter>();
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

var customerRequest = new CreateCustomerRequest("Ada Lovelace", "ADA@EXAMPLE.COM");
var customer = await mediator.Send(customerRequest);

var orderRequest = new CreateTenantOrderRequest(
    customer.Id,
    189.95m);
var order = await mediator.Send(orderRequest);

var persistedCustomers = await dbContext.Customers.CountAsync();
var persistedOrders = await dbContext.TenantOrders.CountAsync();
var persistedInvoices = await dbContext.LegacyInvoices.CountAsync();

Console.WriteLine("TurtlePath commerce sample");
Console.WriteLine($"Default customer CId: {customer.Id}");
Console.WriteLine($"Generated customer CId from factory: {idFactory.New()}");
Console.WriteLine($"Order CId: {order.Id}");
Console.WriteLine($"Legacy invoice CId: {order.LegacyInvoiceId}");
Console.WriteLine($"Persisted rows: customers={persistedCustomers}, orders={persistedOrders}, invoices={persistedInvoices}");
Console.WriteLine($"Audit entries: {auditLog.Entries.Count}");

foreach (var entry in auditLog.Entries)
    Console.WriteLine($"- {entry}");

await sqliteConnection.DisposeAsync();
