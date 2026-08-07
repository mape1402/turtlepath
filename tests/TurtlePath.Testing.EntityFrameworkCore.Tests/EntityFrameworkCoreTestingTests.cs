namespace TurtlePath.Testing.EntityFrameworkCore.Tests
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Pelican.Mediator;
    using TurtlePath.Automations.Profiles;
    using TurtlePath.Commands;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.EntityFrameworkCore;
    using TurtlePath.EntityFrameworkCore.Conventions;
    using TurtlePath.Models.Requests;
    using TurtlePath.Models.Responses;
    using TurtlePath.Testing.EntityFrameworkCore;

    public sealed class EntityFrameworkCoreTestingTests
    {
        [Fact]
        public async Task Sqlite_host_persists_manual_handler_flow()
        {
            await using var host = await TurtlePathTestHost
                .Create()
                .UsePelican(typeof(EntityFrameworkCoreTestingTests).Assembly)
                .UseSqliteDbContext<CommerceTestDbContext>(options => options with
                {
                    ConfigurationAssemblies = [typeof(EntityFrameworkCoreTestingTests).Assembly]
                })
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
                .BuildAsync();

            await host.CreateSchemaAsync<CommerceTestDbContext>();

            var response = await host.SendAsync(new CreateCustomerRequest(7, "Ada"));
            var dbContext = host.Resolve<CommerceTestDbContext>();
            var persisted = await dbContext.Customers.SingleAsync(customer => customer.Id == response.Id);

            Assert.Equal("Ada", persisted.Name);
        }

        [Fact]
        public async Task Sqlite_host_persists_automation_flow()
        {
            await using var host = await TurtlePathTestHost
                .Create()
                .UseAutomations(typeof(EntityFrameworkCoreTestingTests).Assembly)
                .UseSqliteDbContext<CommerceTestDbContext>(options => options with
                {
                    ConfigurationAssemblies = [typeof(EntityFrameworkCoreTestingTests).Assembly]
                })
                .WithMap<CreateAutomatedCustomerRequest, Customer>(request => new Customer
                {
                    Id = request.Id,
                    Name = request.Name
                })
                .WithMap<Customer, CustomerResponse>(customer => new CustomerResponse
                {
                    Id = customer.Id,
                    Name = customer.Name
                })
                .BuildAsync();

            await host.CreateSchemaAsync<CommerceTestDbContext>();

            var response = await host.SendAsync(new CreateAutomatedCustomerRequest(8, "Grace"));
            var dbContext = host.Resolve<CommerceTestDbContext>();
            var persisted = await dbContext.Customers.SingleAsync(customer => customer.Id == response.Id);

            Assert.Equal("Grace", persisted.Name);
        }

        public sealed record CreateCustomerRequest(int Id, string Name) : IRequest<CustomerResponse>;

        public sealed record CreateAutomatedCustomerRequest(int Id, string Name) : IRequest<CustomerResponse>;

        public sealed class Customer : IEntity<int>
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        public sealed class CustomerResponse : IBaseResponse<int>
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        public sealed class CreateCustomerCommandHandler
            : GenericCreateCommandHandler<CreateCustomerRequest, CustomerResponse, Customer, int>
        {
            public CreateCustomerCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }
        }

        private sealed class CustomerAutomationProfile : TurtlePathAutomationProfile
        {
            public override void Configure(ITurtlePathAutomationBuilder builder)
            {
                builder
                    .For<Customer, int>()
                    .ToCreate<CreateAutomatedCustomerRequest, CustomerResponse>();
            }
        }

        public sealed class CommerceTestDbContext : BaseDbContext
        {
            public CommerceTestDbContext(
                DbContextOptions<CommerceTestDbContext> options,
                TurtlePathDbContextOptions turtlePathOptions,
                IEnumerable<ITurtlePathModelConvention> modelConventions)
                : base(options, turtlePathOptions, modelConventions)
            {
            }

            public DbSet<Customer> Customers { get; set; }
        }

        public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
        {
            public void Configure(EntityTypeBuilder<Customer> builder)
            {
                builder.HasKey(customer => customer.Id);
                builder.Property(customer => customer.Name).HasMaxLength(80).IsRequired();
            }
        }
    }
}
