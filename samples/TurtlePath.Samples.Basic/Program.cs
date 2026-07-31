using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TurtlePath.Domain.Identifier;
using TurtlePath.EntityFrameworkCore;
using TurtlePath.Hooks;
using TurtlePath.Samples.Basic.Application.Requests;
using TurtlePath.Samples.Basic.Domain.Entities;
using TurtlePath.Samples.Basic.Domain.Identifier;
using TurtlePath.Samples.Basic.Infrastructure;
using TurtlePath.Samples.Basic.Infrastructure.Persistence;

var sampleAssembly = typeof(Program).Assembly;
var services = new ServiceCollection();

services.AddSingleton<SampleAuditLog>();
services.AddDbContext<CommerceDbContext>(options =>
{
    options.UseInMemoryDatabase("turtlepath-commerce-sample");
});

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
var idFactory = scopedProvider.GetRequiredService<ICIdFactory>();
var idDefinitions = scopedProvider.GetRequiredService<ICIdDefinitionRegistry>();
var auditLog = scopedProvider.GetRequiredService<SampleAuditLog>();

var customerRequest = new CreateCustomerRequest("Ada Lovelace", "ADA@EXAMPLE.COM");
var customer = new Customer
{
    Name = customerRequest.Name,
    Email = customerRequest.Email
};

var customerContext = new CommandHookContext<CreateCustomerRequest, Customer>(customerRequest)
{
    Entity = customer
};

await scopedProvider.RunHooksAsync<IBeforeValidationHook<CreateCustomerRequest, Customer>>(
    hook => hook.BeforeValidationAsync(customerContext));

await scopedProvider.RunHooksAsync<IBeforeSaveHook<CreateCustomerRequest, Customer>>(
    hook => hook.BeforeSaveAsync(customerContext));

var orderRequest = new CreateTenantOrderRequest(
    customer.Id,
    Guid.Parse("87a62326-5f8a-4f5a-9c62-7fa7d11127d5"),
    1001,
    189.95m);

var order = new TenantOrder
{
    Id = CompositeOrderId.Create(orderRequest.TenantId, orderRequest.OrderNumber),
    CustomerId = orderRequest.CustomerId,
    Total = orderRequest.Total
};

var invoiceDefinition = idDefinitions.Get(typeof(LegacyInvoice));
var invoice = new LegacyInvoice
{
    Id = invoiceDefinition.Factory(),
    CustomerId = customer.Id,
    Amount = order.Total
};

dbContext.Customers.Add(customer);
dbContext.TenantOrders.Add(order);
dbContext.LegacyInvoices.Add(invoice);
await dbContext.SaveChangesAsync();

await scopedProvider.RunHooksAsync<IAfterSaveHook<CreateCustomerRequest, Customer>>(
    hook => hook.AfterSaveAsync(customerContext));

var persistedCustomers = await dbContext.Customers.CountAsync();
var persistedOrders = await dbContext.TenantOrders.CountAsync();
var persistedInvoices = await dbContext.LegacyInvoices.CountAsync();

Console.WriteLine("TurtlePath commerce sample");
Console.WriteLine($"Default customer CId: {customer.Id}");
Console.WriteLine($"Generated customer CId from factory: {idFactory.New()}");
Console.WriteLine($"Composite order CId: {order.Id}");
Console.WriteLine($"Legacy invoice CId: {invoice.Id} ({invoiceDefinition.ValueType.Name})");
Console.WriteLine($"Persisted rows: customers={persistedCustomers}, orders={persistedOrders}, invoices={persistedInvoices}");
Console.WriteLine($"Audit entries: {auditLog.Entries.Count}");

foreach (var entry in auditLog.Entries)
    Console.WriteLine($"- {entry}");
