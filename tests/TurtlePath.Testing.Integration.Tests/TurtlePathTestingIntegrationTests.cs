namespace TurtlePath.Testing.Integration.Tests
{
    using global::DataScorpio.Profiles;
    using global::DataScorpio.Testing;
    using DynaBee.Testing;
    using Krackend.EventSourcing.Testing;
    using OctoMap.Testing;
    using Pelican.Testing;
    using Pigeon.Testing;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Testing;

    public sealed class TurtlePathTestingIntegrationTests
    {
        [Fact]
        public async Task Integration_extensions_register_external_testing_adapters()
        {
            await using var host = await TurtlePathTestHost
                .Create()
                .UsePelicanTesting()
                .UseOctoMapTesting()
                .UseCrabalidatorTesting()
                .UsePigeonTesting()
                .UseDynaBeeTesting()
                .UseKrackendTesting()
                .UseDataScorpioTesting(profiles => profiles.AddProfile<CustomerQueryProfile>())
                .BuildAsync();

            Assert.NotNull(host.Resolve<IPelicanTestingAdapter>());
            Assert.NotNull(host.Resolve<IOctoMapTestingAdapter>());
            Assert.NotNull(host.Resolve<IPigeonTestingTransport>());
            Assert.NotNull(host.Resolve<IDynaBeeTestGenerator>());
            Assert.NotNull(host.Resolve<IEventSourcingTestingAdapter>());
            Assert.NotNull(host.Resolve<IEventSourcingTestEventStore>());
            Assert.NotNull(host.Resolve<IDataScorpioTesting<Customer>>());
        }

        private sealed class CustomerQueryProfile : QueryProfile<Customer>
        {
            public override void Configure(IQueryProfileBuilder<Customer> builder)
            {
                builder.AllowFilter(customer => customer.Name);
            }
        }

        private sealed class Customer : IEntity<int>
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
