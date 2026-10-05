namespace TurtlePath.Testing.EventSourcing.Tests
{
    using Krackend.EventSourcing.Contracts;
    using Krackend.EventSourcing.Stores;
    using Krackend.EventSourcing.Streams;
    using Microsoft.Extensions.DependencyInjection;
    using System.Reflection;
    using TurtlePath.EventSourcing;
    using TurtlePath.Hooks;
    using TurtlePath.Mapping;
    using TurtlePath.Testing.EventSourcing;

    public sealed class EventSourcingTestingTests
    {
        [Fact]
        public async Task Host_reads_event_sourcing_stream_for_assertions()
        {
            await using var host = await TurtlePathTestHost
                .Create()
                .ConfigureServices(services =>
                {
                    services
                        .AddTurtlePath()
                        .UseEventSourcingProfile<CustomerEventsProfile>();
                    services.AddSingleton<IMapperAdapter, TestMapperAdapter>();
                })
                .BuildAsync();

            var hook = host.Resolve<IAfterSaveHook<CreateCustomerRequest, Customer>>();
            var context = new CommandHookContext<CreateCustomerRequest, Customer>(
                new CreateCustomerRequest("customer-001", "Ada"))
            {
                Entity = new Customer("customer-001", "Ada")
            };

            await hook.AfterSaveAsync(context);

            var events = await host.ReadEventStreamAsync("customers", "customer-001");

            Assert.Single(events);
            Assert.Equal("customer-created", events.Single().EventType);
            Assert.True(await host.StreamContainsEventAsync("customers", "customer-001", "customer-created"));
        }

        [Fact]
        public async Task Event_sourcing_assertions_validate_host_and_project_missing_values()
        {
            TurtlePathTestHost host = null;
            var getValue = typeof(TurtlePathEventSourcingTestingExtensions)
                .GetMethod("GetValue", BindingFlags.Static | BindingFlags.NonPublic)!
                .MakeGenericMethod(typeof(string));
            var projected = new EventSourcingTestEvent("type", 3, "{}", new Dictionary<string, object>());

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                host.ReadEventStreamAsync("stream", "id"));

            Assert.Equal("type", projected.EventType);
            Assert.Equal(3, projected.StreamVersion);
            Assert.Equal("{}", projected.Payload);
            Assert.Empty(projected.Metadata);
            Assert.Null(getValue.Invoke(null, [null, "Missing"]));
            Assert.Null(getValue.Invoke(null, [new { EventType = 5 }, "EventType"]));
            Assert.Null(getValue.Invoke(null, [new { EventType = "event" }, "Missing"]));
        }

        [EventStream("customers")]
        private sealed record CreateCustomerRequest(string Id, string Name) : IEventStreamCommand
        {
            public string StreamId => Id;
        }

        private sealed record Customer(string Id, string Name);

        [EventSchema("customer-created")]
        private sealed record CustomerCreated(string Id, string Name);

        private sealed class CustomerEventsProfile : IEventSourcingProfile
        {
            public void Configure(IEventSourcingProfileBuilder builder)
            {
                builder.For<CreateCustomerRequest, Customer>()
                    .ToEvent<CustomerCreated>(options => options.UseExpectedVersion(ExpectedVersion.NoStream));
            }
        }

        private sealed class TestMapperAdapter : IMapperAdapter
        {
            public ValueTask<TDestination> MapAsync<TSource, TDestination>(
                TSource source,
                CancellationToken cancellationToken = default)
                where TSource : class
                where TDestination : class
            {
                if (source is EventSourcingMapContext<CreateCustomerRequest, Customer> context &&
                    typeof(TDestination) == typeof(CustomerCreated))
                {
                    return ValueTask.FromResult((TDestination)(object)new CustomerCreated(
                        context.Entity.Id,
                        context.Entity.Name));
                }

                throw new InvalidOperationException($"Unsupported mapping from '{typeof(TSource).Name}' to '{typeof(TDestination).Name}'.");
            }

            public ValueTask UpdateMapAsync<TSource, TDestination>(
                TSource source,
                TDestination destination,
                CancellationToken cancellationToken = default)
                where TSource : class
                where TDestination : class
                => ValueTask.CompletedTask;
        }
    }
}
