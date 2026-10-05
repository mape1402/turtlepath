namespace TurtlePath.DataScorpio.Tests
{
    using global::DataScorpio.Profiles;
    using Microsoft.Extensions.DependencyInjection;
    using TurtlePath;
    using TurtlePath.DataScorpio;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Persistence;

    public sealed class DataScorpioStorageCriteriaApplierTests
    {
        [Fact]
        public void Registration_extensions_register_datascorpio_applier()
        {
            var services = new ServiceCollection();
            var builder = new TestTurtlePathBuilder(services);

            builder.UseDataScorpio(profiles => profiles.AddProfile<CustomerQueryProfile>());

            using var provider = services.BuildServiceProvider();

            Assert.IsType<DataScorpioStorageCriteriaApplier>(provider.GetRequiredService<IStorageCriteriaApplier>());
            Assert.Throws<ArgumentNullException>(() => ((ITurtlePathBuilder)null).UseDataScorpio(_ => { }));
            Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null).AddTurtlePathDataScorpio(_ => { }));
        }

        [Fact]
        public void Apply_uses_datascorpio_filters_and_sorts()
        {
            var services = new ServiceCollection();

            services.AddTurtlePathDataScorpio(profiles => profiles.AddProfile<CustomerQueryProfile>());

            using var provider = services.BuildServiceProvider();
            var applier = provider.GetRequiredService<IStorageCriteriaApplier>();
            var customers = new[]
            {
                new Customer { Id = 1, Name = "Ada", IsActive = true },
                new Customer { Id = 2, Name = "Grace", IsActive = true },
                new Customer { Id = 3, Name = "Adam", IsActive = false }
            };

            var result = applier
                .Apply(customers.AsQueryable(), new GetManyCriteria<Customer>
                {
                    Filters = "Name@=*ada",
                    Sorts = "-Name"
                })
                .ToArray();

            Assert.Equal(["Adam", "Ada"], result.Select(customer => customer.Name));
        }

        [Fact]
        public void Apply_validates_inputs_and_skips_empty_criteria()
        {
            var services = new ServiceCollection();
            services.AddTurtlePathDataScorpio(profiles => profiles.AddProfile<CustomerQueryProfile>());
            using var provider = services.BuildServiceProvider();
            var applier = provider.GetRequiredService<IStorageCriteriaApplier>();
            var customers = new[]
            {
                new Customer { Id = 1, Name = "Ada" }
            }.AsQueryable();

            Assert.Same(customers, applier.Apply(customers, new GetManyCriteria<Customer>()));
            Assert.Throws<ArgumentNullException>(() => new DataScorpioStorageCriteriaApplier(null));
            Assert.Throws<ArgumentNullException>(() => applier.Apply<Customer>(null, new GetManyCriteria<Customer>()));
            Assert.Throws<ArgumentNullException>(() => applier.Apply(customers, null));
        }

        private sealed class CustomerQueryProfile : QueryProfile<Customer>
        {
            public override void Configure(IQueryProfileBuilder<Customer> builder)
            {
                builder
                    .AllowFilter(customer => customer.Name)
                    .AllowSort(customer => customer.Name);
            }
        }

        private sealed class Customer : IEntity<int>
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public bool IsActive { get; set; }
        }

        private sealed class TestTurtlePathBuilder(IServiceCollection services) : ITurtlePathBuilder
        {
            public IServiceCollection Services { get; } = services;
        }
    }
}
