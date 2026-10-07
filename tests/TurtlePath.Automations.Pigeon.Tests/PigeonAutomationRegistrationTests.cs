using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pelican.Mediator;
using Pigeon.Messaging.Consuming.Configuration;
using Pigeon.Messaging.Consuming.Dispatching;
using Pigeon.Messaging.Contracts;
using TurtlePath.Automations.Pigeon;
using TurtlePath.Automations.Profiles;
using TurtlePath.Domain.Contracts;
using TurtlePath.Domain.Identifier;
using TurtlePath.Models.Responses;

namespace TurtlePath.Automations.Pigeon.Tests;

public sealed class PigeonAutomationRegistrationTests
{
    [Fact]
    public async Task AddAutomationConsumers_registers_declared_consumer_and_dispatches_with_mediator()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        services.AddSingleton<CustomerSink>();
        services.AddPelican(typeof(PigeonAutomationRegistrationTests).Assembly);

        services
            .AddPigeon(configuration, _ => { })
            .AddAutomationConsumers(typeof(PigeonAutomationRegistrationTests).Assembly);

        using var provider = services.BuildServiceProvider();
        var consuming = provider.GetRequiredService<IConsumingConfigurator>();
        var consumer = consuming.GetConfiguration(
            "customers.create",
            new SemanticVersion(1, 2, 3),
            "automation-workers");

        Assert.Equal(typeof(CreateCustomerCommand), consumer.MessageType);

        await using var scope = provider.CreateAsyncScope();
        await consumer.Handler(new ConsumeContext
        {
            Services = scope.ServiceProvider,
            Message = new CreateCustomerCommand("mario"),
            CancellationToken = CancellationToken.None
        });

        Assert.Equal("mario", provider.GetRequiredService<CustomerSink>().LastName);
    }

    [Fact]
    public void Consume_stores_consumer_metadata_in_descriptor()
    {
        var descriptor = AutomationDescriptorDiscovery
            .Discover(typeof(PigeonAutomationRegistrationTests).Assembly)
            .Single(item => item.RequestType == typeof(CreateCustomerCommand));

        var options = Assert.IsType<AutomationPigeonConsumerOptions>(
            descriptor.Metadata[AutomationPigeonMetadata.Consumer]);

        Assert.Equal("customers.create", options.Topic);
        Assert.Equal("1.2.3", options.Version);
        Assert.Equal("automation-workers", options.Subscription);
    }

    [Fact]
    public void AddAutomationConsumers_validates_arguments()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ((IPigeonServiceBuilder)null).AddAutomationConsumers(typeof(PigeonAutomationRegistrationTests).Assembly));
    }

    private static IConfiguration CreateConfiguration()
        => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Pigeon:Domain"] = "tests"
            })
            .Build();

    private sealed class Customer : BaseEntity
    {
        public string Name { get; set; }
    }

    public sealed class CustomerResponse : IBaseResponse<CId>
    {
        public CId Id { get; set; }

        public string Name { get; set; }
    }

    public sealed record CreateCustomerCommand(string Name) : IRequest<CustomerResponse>;

    private sealed class CustomerAutomationProfile : TurtlePathAutomationProfile
    {
        public override void Configure(ITurtlePathAutomationBuilder builder)
        {
            builder.For<Customer>()
                .ToCreate<CreateCustomerCommand, CustomerResponse>(operation => operation
                    .Consume("customers.create", "1.2.3", "automation-workers"));
        }
    }

    public sealed class CustomerSink
    {
        public string LastName { get; set; }
    }

    public sealed class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerResponse>
    {
        private readonly CustomerSink sink;

        public CreateCustomerCommandHandler(CustomerSink sink)
        {
            this.sink = sink;
        }

        public Task<CustomerResponse> Handle(CreateCustomerCommand request, CancellationToken cancellationToken = default)
        {
            sink.LastName = request.Name;
            return Task.FromResult(new CustomerResponse
            {
                Id = CId.From("customer-1"),
                Name = request.Name
            });
        }
    }
}
