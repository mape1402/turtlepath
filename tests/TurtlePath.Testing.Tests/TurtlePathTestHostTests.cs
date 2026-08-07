namespace TurtlePath.Testing.Tests
{
    using Pelican.Mediator;
    using TurtlePath.Automations.Profiles;
    using TurtlePath.Commands;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Models.Responses;
    using TurtlePath.Persistence;
    using TurtlePath.Testing.Hooks;
    using TurtlePath.Testing.Persistence;

    public class TurtlePathTestHostTests
    {
        [Fact]
        public async Task Host_resolves_manual_handler_without_mocking_dependencies()
        {
            await using var host = await TurtlePathTestHost
                .Create()
                .WithMap<CreateCustomerRequest, Customer>(request => new Customer
                {
                    Id = 1,
                    Name = request.Name
                })
                .WithMap<Customer, CustomerResponse>(customer => new CustomerResponse
                {
                    Id = customer.Id,
                    Name = customer.Name
                })
                .TraceHooks()
                .BuildAsync();

            var handler = new CreateCustomerCommandHandler(host.Services);

            var response = await handler.Handle(new CreateCustomerRequest("Ada"));

            Assert.Equal(1, response.Id);
            Assert.Equal("Ada", response.Name);
            Assert.True(host.Store<Customer>().Contains(customer => customer.Name == "Ada"));
            Assert.Contains(host.Storage.Operations, operation => operation.Action == "SaveChanges");
            Assert.Equal(
                ["BeforeValidation", "AfterValidation", "BeforeMap", "AfterMap", "BeforeSave", "AfterSave", "BeforeResponse", "AfterResponse"],
                host.Resolve<HookTrace>().Entries.Select(entry => entry.Stage));
        }

        [Fact]
        public async Task Host_sends_manual_handler_through_pelican()
        {
            await using var host = await TurtlePathTestHost
                .Create()
                .UsePelican(typeof(TurtlePathTestHostTests).Assembly)
                .WithMap<CreateCustomerRequest, Customer>(request => new Customer
                {
                    Id = 2,
                    Name = request.Name
                })
                .WithMap<Customer, CustomerResponse>(customer => new CustomerResponse
                {
                    Id = customer.Id,
                    Name = customer.Name
                })
                .BuildAsync();

            var response = await host.SendAsync(new CreateCustomerRequest("Grace"));

            Assert.Equal(2, response.Id);
            Assert.True(host.Store<Customer>().Contains(customer => customer.Name == "Grace"));
        }

        [Fact]
        public async Task Host_sends_automation_through_pelican_with_in_memory_storage()
        {
            await using var host = await TurtlePathTestHost
                .Create()
                .UseAutomations(typeof(TurtlePathTestHostTests).Assembly)
                .WithMap<CreateAutomatedCustomerRequest, Customer>(request => new Customer
                {
                    Id = 3,
                    Name = request.Name
                })
                .WithMap<Customer, CustomerResponse>(customer => new CustomerResponse
                {
                    Id = customer.Id,
                    Name = customer.Name
                })
                .BuildAsync();

            var response = await host.SendAsync(new CreateAutomatedCustomerRequest("Linus"));

            Assert.Equal(3, response.Id);
            Assert.True(host.Store<Customer>().Contains(customer => customer.Name == "Linus"));
        }

        [Fact]
        public async Task Host_reads_seeded_entities_through_storage_adapter()
        {
            var customer = new Customer
            {
                Id = 42,
                Name = "Margaret"
            };

            await using var host = await TurtlePathTestHost
                .Create()
                .WithSeed(customer)
                .WithMap<Customer, CustomerResponse>(entity => new CustomerResponse
                {
                    Id = entity.Id,
                    Name = entity.Name
                })
                .BuildAsync();

            var reader = host.Resolve<IStorageReaderAdapter>();
            var response = await reader
                .For<Customer>()
                .Where(entity => entity.Id == 42)
                .FirstOrDefaultAsync<CustomerResponse>();

            Assert.NotNull(response);
            Assert.Equal("Margaret", response.Name);
        }

        public sealed record CreateCustomerRequest(string Name) : IRequest<CustomerResponse>;

        public sealed record CreateAutomatedCustomerRequest(string Name) : IRequest<CustomerResponse>;

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
    }
}
