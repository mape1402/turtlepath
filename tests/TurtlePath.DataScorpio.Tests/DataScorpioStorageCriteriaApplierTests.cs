namespace TurtlePath.DataScorpio.Tests
{
    using global::DataScorpio.Profiles;
    using Microsoft.Extensions.DependencyInjection;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Persistence;

    public sealed class DataScorpioStorageCriteriaApplierTests
    {
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
    }
}
