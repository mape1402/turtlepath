namespace TurtlePath.Samples.Testing
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using DataScorpio.Profiles;
    using DataScorpio.Testing;
    using Pelican.Mediator;
    using TurtlePath.Automations.Profiles;
    using TurtlePath.Commands;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.EntityFrameworkCore;
    using TurtlePath.EntityFrameworkCore.Conventions;
    using TurtlePath.ExceptionHandling;
    using TurtlePath.Jobs;
    using TurtlePath.Models.Requests;
    using TurtlePath.Models.Responses;
    using TurtlePath.Queries;
    using TurtlePath.Testing;
    using TurtlePath.Testing.EntityFrameworkCore;
    using TurtlePath.Testing.Hooks;
    using TurtlePath.Testing.Integration;

    public sealed class TestingExamples
    {
        [Fact]
        public async Task Unit_test_a_manual_handler_without_mocks()
        {
            await using var host = await TurtlePathTestHost
                .Create()
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
                .TraceHooks()
                .BuildAsync();

            var handler = new CreateCustomerCommandHandler(host.Services);

            var response = await handler.Handle(new CreateCustomerRequest(1, "Ada"));

            Assert.Equal("Ada", response.Name);
            Assert.True(host.Store<Customer>().Contains(customer => customer.Id == 1));
            Assert.Contains(host.Resolve<HookTrace>().Entries, entry => entry.Stage == "AfterSave");
        }

        [Fact]
        public async Task Integration_test_an_automation_with_sqlite()
        {
            await using var host = await TurtlePathTestHost
                .Create()
                .UseAutomations(typeof(TestingExamples).Assembly)
                .UseSqliteDbContext<SampleDbContext>(options => options with
                {
                    ConfigurationAssemblies = [typeof(TestingExamples).Assembly]
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

            await host.CreateSchemaAsync<SampleDbContext>();

            var response = await host.SendAsync(new CreateAutomatedCustomerRequest(2, "Grace"));
            var dbContext = host.Resolve<SampleDbContext>();

            Assert.Equal("Grace", response.Name);
            Assert.True(await dbContext.Customers.AnyAsync(customer => customer.Id == 2));
        }

        [Fact]
        public async Task Integration_test_query_handlers_with_seeded_storage()
        {
            await using var host = await TurtlePathTestHost
                .Create()
                .UsePelican(typeof(TestingExamples).Assembly)
                .WithSeed(
                    new Customer { Id = 1, Name = "Ada" },
                    new Customer { Id = 2, Name = "Grace" })
                .WithMap<Customer, CustomerResponse>(customer => new CustomerResponse
                {
                    Id = customer.Id,
                    Name = customer.Name
                })
                .BuildAsync();

            var page = await host.SendAsync(new GetCustomersPageQuery(new PagedSettings
            {
                PageNumber = 1,
                PageSize = 10
            }));

            Assert.Equal(2, page.RowCount);
        }

        [Fact]
        public async Task Integration_test_datascorpio_helpers_from_turtlepath_host()
        {
            await using var host = await TurtlePathTestHost
                .Create()
                .UseDataScorpioTesting(profiles => profiles.AddProfile<CustomerQueryProfile>())
                .BuildAsync();

            var dataScorpio = host.Resolve<IDataScorpioTesting<Customer>>();

            await dataScorpio.SeedAsync(
            [
                new Customer { Id = 1, Name = "Ada" },
                new Customer { Id = 2, Name = "Grace" },
                new Customer { Id = 3, Name = "Adam" }
            ]);

            var result = await dataScorpio.ApplyAsync(filters: "Name@=*ada", sorts: "-Name");

            Assert.True(result.IsSuccess);
            Assert.Equal(["Adam", "Ada"], result.Result.Items.Select(customer => customer.Name));
        }

        [Fact]
        public async Task Test_exception_handling_and_jobs()
        {
            SampleJob.Executions = 0;

            await using var host = await TurtlePathTestHost
                .Create()
                .UseExceptionHandling(builder =>
                {
                    builder.For<InvalidOperationException>(
                        ExceptionKind.Business,
                        exception => exception.Message);
                })
                .UseJobs()
                .WithJob<SampleJob>()
                .BuildAsync();

            var exception = host.HandleException(new InvalidOperationException("Sample failure."));
            var jobs = await host.RunJobsAsync();

            Assert.Equal(ExceptionKind.Business, exception.Descriptor.Kind);
            Assert.True(jobs.Succeeded);
            Assert.Equal(1, SampleJob.Executions);
        }

        public sealed record CreateCustomerRequest(int Id, string Name) : IRequest<CustomerResponse>;

        public sealed record CreateAutomatedCustomerRequest(int Id, string Name) : IRequest<CustomerResponse>;

        public sealed class UpdateCustomerRequest : IBaseRequest<int>, IRequest<CustomerResponse>
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

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

        public sealed class GetCustomersPageQuery : GenericGetPagedInfoQuery<Customer, CustomerResponse, int>
        {
            public GetCustomersPageQuery(PagedSettings pagedSettings) : base(pagedSettings)
            {
            }
        }

        public sealed class GetCustomersPageQueryHandler
            : GenericGetPagedInfoQueryHandler<GetCustomersPageQuery, Customer, CustomerResponse, int>
        {
            public GetCustomersPageQueryHandler(IServiceProvider serviceProvider) : base(serviceProvider)
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

        private sealed class CustomerQueryProfile : QueryProfile<Customer>
        {
            public override void Configure(IQueryProfileBuilder<Customer> builder)
            {
                builder
                    .AllowFilter(customer => customer.Name)
                    .AllowSort(customer => customer.Name);
            }
        }

        public sealed class SampleDbContext : BaseDbContext
        {
            public SampleDbContext(
                DbContextOptions<SampleDbContext> options,
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

        public sealed class SampleJob : TurtlePathJob
        {
            public static int Executions;

            public override Task ExecuteAsync(TurtlePathJobContext context, CancellationToken cancellationToken)
            {
                Interlocked.Increment(ref Executions);
                return Task.CompletedTask;
            }
        }
    }
}
