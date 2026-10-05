using Krackend.EventSourcing.Contracts;
using Krackend.EventSourcing.Stores;
using Krackend.EventSourcing.Streams;
using Microsoft.Extensions.DependencyInjection;
using TurtlePath.EventSourcing;
using TurtlePath.Hooks;
using TurtlePath.Mapping;

namespace TurtlePath.EventSourcing.Tests;

public class EventSourcingTests
{
    [Fact]
    public async Task EventSourcingAfterSaveHook_appends_multiple_events_from_profile()
    {
        var services = new ServiceCollection();

        services
            .AddTurtlePath()
            .UseEventSourcingProfile<CustomerEventSourcingProfile>();
        services.AddSingleton<IMapperAdapter, TestMapperAdapter>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var hooks = scope.ServiceProvider
            .GetServices<IAfterSaveHook<CreateCustomerRequest, Customer>>()
            .ToArray();

        Assert.Single(hooks);

        var context = new CommandHookContext<CreateCustomerRequest, Customer>(
            new CreateCustomerRequest("customer-001", "Ada"))
        {
            Entity = new Customer("customer-001", "Ada")
        };

        await hooks[0].AfterSaveAsync(context);

        var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();
        var envelopes = await eventStore.ReadStreamAsync("customers", "customer-001", 1, 10);

        Assert.Equal(2, envelopes.Count);
        Assert.Contains(envelopes, envelope => envelope.EventType == "customer-created");
        Assert.Contains(envelopes, envelope => envelope.EventType == "customer-audited");
    }

    [Fact]
    public async Task EventSourcingProfile_can_skip_events_with_condition()
    {
        var services = new ServiceCollection();

        services
            .AddTurtlePath()
            .UseEventSourcingProfile(new ConditionalCustomerEventSourcingProfile());
        services.AddSingleton<IMapperAdapter, TestMapperAdapter>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var hook = scope.ServiceProvider
            .GetRequiredService<IAfterSaveHook<CreateCustomerRequest, Customer>>();

        var context = new CommandHookContext<CreateCustomerRequest, Customer>(
            new CreateCustomerRequest("customer-002", "Skip audit"))
        {
            Entity = new Customer("customer-002", "Skip audit")
        };

        await hook.AfterSaveAsync(context);

        var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();
        var envelopes = await eventStore.ReadStreamAsync("customers", "customer-002", 1, 10);

        Assert.Single(envelopes);
        Assert.Contains(envelopes, envelope => envelope.EventType == "customer-created");
        Assert.DoesNotContain(envelopes, envelope => envelope.EventType == "customer-audited");
    }

    [Fact]
    public void EventSourcingProfiles_discovers_profiles_from_assemblies()
    {
        var services = new ServiceCollection();

        services
            .AddTurtlePath()
            .UseEventSourcingProfiles(typeof(CustomerEventSourcingProfile).Assembly);
        services.AddSingleton<IMapperAdapter, TestMapperAdapter>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var hooks = scope.ServiceProvider
            .GetServices<IAfterSaveHook<CreateCustomerRequest, Customer>>()
            .ToArray();

        Assert.Single(hooks);
    }

    [Fact]
    public async Task EventSourcingProfile_can_resolve_stream_from_entity_and_map_from_custom_source()
    {
        var services = new ServiceCollection();

        services
            .AddTurtlePath()
            .UseEventSourcingProfile<EntityStreamEventSourcingProfile>();
        services.AddSingleton<IMapperAdapter, TestMapperAdapter>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var hook = scope.ServiceProvider
            .GetRequiredService<IAfterSaveHook<EntityStreamCreateCustomerRequest, Customer>>();

        var context = new CommandHookContext<EntityStreamCreateCustomerRequest, Customer>(
            new EntityStreamCreateCustomerRequest("Ignored stream id"))
        {
            Entity = new Customer("customer-from-entity", "Entity Stream")
        };

        await hook.AfterSaveAsync(context);

        var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();
        var envelopes = await eventStore.ReadStreamAsync("customers", "customer-from-entity", 1, 10);

        Assert.Single(envelopes);
        Assert.Contains(envelopes, envelope => envelope.EventType == "customer-created");
    }

    [Fact]
    public async Task EventSourcingAfterSaveHook_notifies_append_observers_with_envelopes()
    {
        var services = new ServiceCollection();

        services
            .AddTurtlePath()
            .UseEventSourcingProfile<MixedExpectedVersionCustomerEventSourcingProfile>();
        services.AddSingleton<IMapperAdapter, TestMapperAdapter>();
        services.AddScoped<RecordingAppendObserver>();
        services.AddScoped<IEventSourcingAppendObserver>(provider =>
            provider.GetRequiredService<RecordingAppendObserver>());
        services.AddScoped<IEventSourcingAppendObserver<CreateCustomerRequest, Customer>>(provider =>
            provider.GetRequiredService<RecordingAppendObserver>());

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var observer = scope.ServiceProvider.GetRequiredService<RecordingAppendObserver>();
        var hook = scope.ServiceProvider
            .GetRequiredService<IAfterSaveHook<CreateCustomerRequest, Customer>>();

        var request = new CreateCustomerRequest("customer-003", "Observer");
        var entity = new Customer("customer-003", "Observer");
        var context = new CommandHookContext<CreateCustomerRequest, Customer>(request)
        {
            Entity = entity
        };

        await hook.AfterSaveAsync(context);

        Assert.Equal(2, observer.TypedContexts.Count);
        Assert.Equal(2, observer.Contexts.Count);

        var firstTypedContext = observer.TypedContexts[0];

        Assert.Same(request, firstTypedContext.Request);
        Assert.Same(entity, firstTypedContext.Entity);
        Assert.Equal("customers", firstTypedContext.StreamName);
        Assert.Equal("customer-003", firstTypedContext.StreamId);
        Assert.Equal(ExpectedVersion.NoStream, firstTypedContext.ExpectedVersion);
        Assert.Single(firstTypedContext.Payloads);
        Assert.IsType<CustomerCreated>(firstTypedContext.Payloads.Single());
        Assert.Single(firstTypedContext.Envelopes);
        Assert.False(string.IsNullOrWhiteSpace(firstTypedContext.Envelopes.Single().EventId));
        Assert.Equal("customer-created", firstTypedContext.Envelopes.Single().EventType);
        Assert.Equal(1, firstTypedContext.Envelopes.Single().StreamVersion);

        var secondContext = observer.Contexts[1];

        Assert.Same(request, secondContext.Request);
        Assert.Same(entity, secondContext.Entity);
        Assert.Equal("customers", secondContext.StreamName);
        Assert.Equal("customer-003", secondContext.StreamId);
        Assert.Equal(ExpectedVersion.Any, secondContext.ExpectedVersion);
        Assert.Single(secondContext.Payloads);
        Assert.IsType<CustomerAudited>(secondContext.Payloads.Single());
        Assert.Single(secondContext.Envelopes);
        Assert.Equal("customer-audited", secondContext.Envelopes.Single().EventType);
        Assert.Equal(2, secondContext.Envelopes.Single().StreamVersion);
    }

    private sealed record EntityStreamCreateCustomerRequest(string Name);

    [EventStream("customers")]
    private sealed record CreateCustomerRequest(string Id, string Name) : IEventStreamCommand
    {
        public string StreamId => Id;
    }

    private sealed record Customer(string Id, string Name);

    [EventSchema("customer-created")]
    private sealed record CustomerCreated(string Id, string Name);

    [EventSchema("customer-audited")]
    private sealed record CustomerAudited(string Id);

    private sealed record CustomerEventSource(string Id, string Name);

    private sealed class CustomerEventSourcingProfile : IEventSourcingProfile
    {
        public void Configure(IEventSourcingProfileBuilder builder)
        {
            builder.For<CreateCustomerRequest, Customer>()
                .ToEvent<CustomerCreated>(options => options.UseExpectedVersion(ExpectedVersion.NoStream))
                .ToEvent<CustomerAudited>(options => options.UseExpectedVersion(ExpectedVersion.NoStream));
        }
    }

    private sealed class ConditionalCustomerEventSourcingProfile : IEventSourcingProfile
    {
        public void Configure(IEventSourcingProfileBuilder builder)
        {
            builder.For<CreateCustomerRequest, Customer>()
                .ToEvent<CustomerCreated>()
                .ToEvent<CustomerAudited>(options => options.When(_ => false));
        }
    }

    private sealed class EntityStreamEventSourcingProfile : IEventSourcingProfile
    {
        public void Configure(IEventSourcingProfileBuilder builder)
        {
            builder.For<EntityStreamCreateCustomerRequest, Customer>()
                .UseStream("customers", context => context.Entity.Id)
                .ToEvent<CustomerEventSource, CustomerCreated>(
                    context => new CustomerEventSource(context.Entity.Id, context.Entity.Name));
        }
    }

    private sealed class MixedExpectedVersionCustomerEventSourcingProfile : IEventSourcingProfile
    {
        public void Configure(IEventSourcingProfileBuilder builder)
        {
            builder.For<CreateCustomerRequest, Customer>()
                .ToEvent<CustomerCreated>(options => options.UseExpectedVersion(ExpectedVersion.NoStream))
                .ToEvent<CustomerAudited>(options => options.UseExpectedVersion(ExpectedVersion.Any));
        }
    }

    private sealed class RecordingAppendObserver :
        IEventSourcingAppendObserver,
        IEventSourcingAppendObserver<CreateCustomerRequest, Customer>
    {
        public List<EventSourcingAppendContext> Contexts { get; } = new();

        public List<EventSourcingAppendContext<CreateCustomerRequest, Customer>> TypedContexts { get; } = new();

        public ValueTask OnAppendedAsync(
            EventSourcingAppendContext context,
            CancellationToken cancellationToken = default)
        {
            Contexts.Add(context);
            return ValueTask.CompletedTask;
        }

        public ValueTask OnAppendedAsync(
            EventSourcingAppendContext<CreateCustomerRequest, Customer> context,
            CancellationToken cancellationToken = default)
        {
            TypedContexts.Add(context);
            return ValueTask.CompletedTask;
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
            if (source is EventSourcingMapContext<CreateCustomerRequest, Customer> context)
            {
                object mapped = typeof(TDestination) == typeof(CustomerCreated)
                    ? new CustomerCreated(context.Entity.Id, context.Entity.Name)
                    : new CustomerAudited(context.Entity.Id);

                return ValueTask.FromResult((TDestination)mapped);
            }

            if (source is CustomerEventSource eventSource)
            {
                object mapped = new CustomerCreated(eventSource.Id, eventSource.Name);

                return ValueTask.FromResult((TDestination)mapped);
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
