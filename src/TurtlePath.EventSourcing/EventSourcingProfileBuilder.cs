namespace TurtlePath.EventSourcing
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Krackend.EventSourcing.Registry;
    using Krackend.EventSourcing.Streams;
    using TurtlePath.EventSourcing.Internal;
    using TurtlePath.Hooks;

    /// <inheritdoc />
    public sealed class EventSourcingProfileBuilder : IEventSourcingProfileBuilder
    {
        private readonly IServiceCollection services;
        private readonly EventSourcingRegistrationRegistry registry;
        private readonly HashSet<System.Reflection.Assembly> eventAssemblies = new();

        internal EventSourcingProfileBuilder(
            IServiceCollection services,
            EventSourcingRegistrationRegistry registry)
        {
            this.services = services ?? throw new ArgumentNullException(nameof(services));
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        /// <inheritdoc />
        public IEventSourcingEntityBuilder<TRequest, TEntity> For<TRequest, TEntity>()
            where TRequest : class
            where TEntity : class
            => new EventSourcingEntityBuilder<TRequest, TEntity>(services, registry, eventAssemblies);

        internal IReadOnlyCollection<System.Reflection.Assembly> EventAssemblies => eventAssemblies;

        private sealed class EventSourcingEntityBuilder<TRequest, TEntity>
            : IEventSourcingEntityBuilder<TRequest, TEntity>
            where TRequest : class
            where TEntity : class
        {
            private readonly IServiceCollection services;
            private readonly EventSourcingRegistrationRegistry registry;
            private readonly HashSet<System.Reflection.Assembly> eventAssemblies;

            public EventSourcingEntityBuilder(
                IServiceCollection services,
                EventSourcingRegistrationRegistry registry,
                HashSet<System.Reflection.Assembly> eventAssemblies)
            {
                this.services = services;
                this.registry = registry;
                this.eventAssemblies = eventAssemblies;
            }

            public IEventSourcingEntityBuilder<TRequest, TEntity> ToEvent<TEvent>(
                Action<EventSourcingEventOptions<TRequest, TEntity>> configure = null)
                where TEvent : class
            {
                var options = new EventSourcingEventOptions<TRequest, TEntity>();
                configure?.Invoke(options);

                registry.Add(EventSourcingRegistration<TRequest, TEntity>.Create<TEvent>(options));
                eventAssemblies.Add(typeof(TEvent).Assembly);
                RegisterEventTypeIfRegistryExists<TEvent>();
                services.TryAddEnumerable(ServiceDescriptor.Scoped<
                    IAfterSaveHook<TRequest, TEntity>,
                    EventSourcingAfterSaveHook<TRequest, TEntity>>());

                return this;
            }

            public IEventSourcingEntityBuilder<TRequest, TEntity> UseStream(
                string streamName,
                Func<CommandHookContext<TRequest, TEntity>, string> streamId)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(streamName);

                if (streamId == null)
                    throw new ArgumentNullException(nameof(streamId));

                registry.SetStream(new EventSourcingStreamConfiguration<TRequest, TEntity>(
                    context => EventStreamReference.Create(streamName, streamId(context))));

                return this;
            }

            public IEventSourcingEntityBuilder<TRequest, TEntity> ToEvent<TSource, TEvent>(
                Func<CommandHookContext<TRequest, TEntity>, TSource> source,
                Action<EventSourcingEventOptions<TRequest, TEntity>> configure = null)
                where TSource : class
                where TEvent : class
            {
                if (source == null)
                    throw new ArgumentNullException(nameof(source));

                var options = new EventSourcingEventOptions<TRequest, TEntity>();
                configure?.Invoke(options);

                registry.Add(EventSourcingRegistration<TRequest, TEntity>.Create<TSource, TEvent>(source, options));
                eventAssemblies.Add(typeof(TEvent).Assembly);
                RegisterEventTypeIfRegistryExists<TEvent>();
                services.TryAddEnumerable(ServiceDescriptor.Scoped<
                    IAfterSaveHook<TRequest, TEntity>,
                    EventSourcingAfterSaveHook<TRequest, TEntity>>());

                return this;
            }

            private void RegisterEventTypeIfRegistryExists<TEvent>()
            {
                var registry = services
                    .Select(descriptor => descriptor.ImplementationInstance)
                    .OfType<EventTypeRegistry>()
                    .FirstOrDefault();

                registry?.Register<TEvent>();
            }
        }
    }
}
