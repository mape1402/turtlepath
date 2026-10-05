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
                .UseOctoMapTesting(_ => { })
                .UseCrabalidatorTesting()
                .UsePigeonTesting()
                .UseSpiderTesting(typeof(TurtlePathTestingIntegrationTests).Assembly)
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

        [Fact]
        public async Task Integration_extensions_register_sqlite_datascorpio_testing()
        {
            await using var host = await TurtlePathTestHost
                .Create()
                .UseDataScorpioSqliteTesting(profiles => profiles.AddProfile<CustomerQueryProfile>())
                .BuildAsync();

            Assert.NotNull(host.Resolve<IDataScorpioTesting<Customer>>());
        }

        [Fact]
        public void Integration_extensions_validate_builder_arguments()
        {
            TurtlePathTestHostBuilder builder = null;

            Assert.Throws<ArgumentNullException>(() => builder.UsePelicanTesting());
            Assert.Throws<ArgumentNullException>(() => builder.UseOctoMapTesting());
            Assert.Throws<ArgumentNullException>(() => builder.UseOctoMapTesting(_ => { }));
            Assert.Throws<ArgumentNullException>(() => builder.UseCrabalidatorTesting());
            Assert.Throws<ArgumentNullException>(() => builder.UsePigeonTesting());
            Assert.Throws<ArgumentNullException>(() => builder.UseSpiderTesting());
            Assert.Throws<ArgumentNullException>(() => builder.UseDynaBeeTesting());
            Assert.Throws<ArgumentNullException>(() => builder.UseKrackendTesting());
            Assert.Throws<ArgumentNullException>(() => builder.UseKrackendEventSourcingTesting());
            Assert.Throws<ArgumentNullException>(() => builder.UseDataScorpioTesting(_ => { }));
            Assert.Throws<ArgumentNullException>(() => builder.UseDataScorpioSqliteTesting(_ => { }));
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
