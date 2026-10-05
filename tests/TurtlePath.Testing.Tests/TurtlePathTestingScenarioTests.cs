namespace TurtlePath.Testing.Tests
{
    using Microsoft.Extensions.DependencyInjection;
    using Pelican.Mediator;
    using TurtlePath.Commands;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.ExceptionHandling;
    using TurtlePath.Jobs;
    using TurtlePath.Models.Requests;
    using TurtlePath.Models.Responses;
    using TurtlePath.Queries;
    using TurtlePath.Testing.Hooks;

    public sealed class TurtlePathTestingScenarioTests
    {
        [Fact]
        public async Task Host_supports_update_delete_and_paged_handler_scenarios()
        {
            await using var host = await TurtlePathTestHost
                .Create()
                .UsePelican(typeof(TurtlePathTestingScenarioTests).Assembly)
                .TraceHooks()
                .WithSeed(
                    new Product { Id = 1, Name = "One" },
                    new Product { Id = 2, Name = "Two" },
                    new Product { Id = 3, Name = "Three" })
                .WithUpdateMap<UpdateProductRequest, Product>((request, product) => product.Name = request.Name)
                .WithMap<Product, ProductResponse>(product => new ProductResponse
                {
                    Id = product.Id,
                    Name = product.Name
                })
                .BuildAsync();

            var updated = await host.SendAsync(new UpdateProductRequest { Id = 2, Name = "Updated" });
            var patched = await host.SendAsync(new PatchProductRequest { Id = 3, Name = "Patched" });
            var page = await host.SendAsync(new GetProductsPageQuery(new PagedSettings { PageNumber = 1, PageSize = 2 }));
            var deleted = await host.SendAsync(new DeleteProductRequest { Id = 1 });

            Assert.Equal("Updated", updated.Name);
            Assert.Equal("Patched", patched.Name);
            Assert.Equal(2, page.Results.Count());
            Assert.Equal("One", deleted.Name);
            Assert.False(host.Store<Product>().Contains(product => product.Id == 1));
            Assert.Contains(host.Resolve<HookTrace>().Entries, entry =>
                entry.Stage == "BeforeQuery" &&
                entry.RequestType == typeof(GetProductsPageQuery) &&
                entry.ResponseType == typeof(PagedResponse<ProductResponse>));
            Assert.Contains(host.Resolve<HookTrace>().Entries, entry =>
                entry.Stage == "AfterQuery" &&
                entry.Response is PagedResponse<ProductResponse>);
            Assert.Contains(host.Resolve<HookTrace>().Entries, entry =>
                entry.Stage == "BeforePatch" &&
                entry.Request is PatchProductRequest &&
                entry.Entity is Product);
            Assert.Contains(host.Resolve<HookTrace>().Entries, entry => entry.Stage == "AfterPatch");
        }

        [Fact]
        public async Task Host_supports_no_response_handlers()
        {
            await using var host = await TurtlePathTestHost
                .Create()
                .WithMap<CreateProductNoResponseRequest, Product>(request => new Product
                {
                    Id = request.Id,
                    Name = request.Name
                })
                .BuildAsync();

            var handler = new CreateProductNoResponseCommandHandler(host.Services);

            await handler.Handle(new CreateProductNoResponseRequest(10, "No response"));

            Assert.True(host.Store<Product>().Contains(product => product.Id == 10));
        }

        [Fact]
        public async Task Host_supports_jobs()
        {
            CountingJob.Executions = 0;

            await using var host = await TurtlePathTestHost
                .Create()
                .UseExceptionHandling()
                .UseJobs()
                .WithJob<CountingJob>()
                .BuildAsync();

            var result = await host.RunJobsAsync();

            Assert.True(result.Succeeded);
            Assert.Equal(1, CountingJob.Executions);
        }

        [Fact]
        public async Task Host_registers_cron_jobs_for_testing()
        {
            await using var host = await TurtlePathTestHost
                .Create()
                .UseExceptionHandling()
                .WithCronJob<CountingJob>(options => options.EveryHours(1), "counting-cron")
                .BuildAsync();

            var definition = Assert.Single(host.Services.GetServices<TurtlePathCronJobDefinition>());

            Assert.Equal("counting-cron", definition.Name);
            Assert.Equal(TimeSpan.FromHours(1), definition.Options.Interval);
        }

        [Fact]
        public async Task Host_supports_exception_handling_assertions()
        {
            await using var host = await TurtlePathTestHost
                .Create()
                .UseExceptionHandling(builder =>
                {
                    builder.For<InvalidOperationException>(
                        ExceptionKind.Business,
                        exception => exception.Message);
                })
                .BuildAsync();

            var result = host.HandleException(new InvalidOperationException("Business failed."));

            Assert.Equal(ExceptionKind.Business, result.Descriptor.Kind);
            Assert.Equal("Business failed.", result.Descriptor.Messages.Single());
        }

        public sealed class Product : IEntity<int>
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        public sealed class ProductResponse : IBaseResponse<int>
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        public sealed class UpdateProductRequest : IBaseRequest<int>, IRequest<ProductResponse>
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        public sealed class DeleteProductRequest : IBaseRequest<int>, IRequest<ProductResponse>
        {
            public int Id { get; set; }
        }

        public sealed class PatchProductRequest : IBaseRequest<int>, IRequest<ProductResponse>, IPatchAction<Product>
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public ValueTask PatchAsync(Product entity, CancellationToken cancellationToken)
            {
                entity.Name = Name;
                return ValueTask.CompletedTask;
            }
        }

        public sealed record CreateProductNoResponseRequest(int Id, string Name) : IRequest;

        public sealed class GetProductsPageQuery : GenericGetPagedInfoQuery<Product, ProductResponse, int>
        {
            public GetProductsPageQuery(PagedSettings pagedSettings) : base(pagedSettings)
            {
            }
        }

        public sealed class UpdateProductCommandHandler
            : GenericUpdateCommandHandler<UpdateProductRequest, ProductResponse, Product, int>
        {
            public UpdateProductCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }
        }

        public sealed class DeleteProductCommandHandler
            : GenericDeleteCommandHandler<DeleteProductRequest, ProductResponse, Product, int>
        {
            public DeleteProductCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }
        }

        public sealed class PatchProductCommandHandler
            : GenericPatchCommandHandler<PatchProductRequest, ProductResponse, Product, int>
        {
            public PatchProductCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }
        }

        public sealed class CreateProductNoResponseCommandHandler
            : GenericCreateCommandHandler<CreateProductNoResponseRequest, Product, int>
        {
            public CreateProductNoResponseCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }
        }

        public sealed class GetProductsPageQueryHandler
            : GenericGetPagedInfoQueryHandler<GetProductsPageQuery, Product, ProductResponse, int>
        {
            public GetProductsPageQueryHandler(IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }
        }

        public sealed class CountingJob : TurtlePathJob
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
