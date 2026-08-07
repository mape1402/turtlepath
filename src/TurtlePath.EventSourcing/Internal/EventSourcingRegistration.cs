namespace TurtlePath.EventSourcing.Internal
{
    using Krackend.EventSourcing.Stores;
    using Microsoft.Extensions.DependencyInjection;
    using TurtlePath.Hooks;
    using TurtlePath.Mapping;

    internal sealed class EventSourcingRegistration<TRequest, TEntity>
        where TRequest : class
        where TEntity : class
    {
        public EventSourcingRegistration(
            Type eventType,
            Func<IServiceProvider, CommandHookContext<TRequest, TEntity>, CancellationToken, ValueTask<object>> map,
            Func<CommandHookContext<TRequest, TEntity>, ExpectedVersion> expectedVersion,
            Func<CommandHookContext<TRequest, TEntity>, bool> shouldAppend)
        {
            EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
            Map = map ?? throw new ArgumentNullException(nameof(map));
            ExpectedVersion = expectedVersion ?? throw new ArgumentNullException(nameof(expectedVersion));
            ShouldAppend = shouldAppend ?? throw new ArgumentNullException(nameof(shouldAppend));
        }

        public Type EventType { get; }

        public Func<IServiceProvider, CommandHookContext<TRequest, TEntity>, CancellationToken, ValueTask<object>> Map { get; }

        public Func<CommandHookContext<TRequest, TEntity>, ExpectedVersion> ExpectedVersion { get; }

        public Func<CommandHookContext<TRequest, TEntity>, bool> ShouldAppend { get; }

        public static EventSourcingRegistration<TRequest, TEntity> Create<TEvent>(
            EventSourcingEventOptions<TRequest, TEntity> options)
            where TEvent : class
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return new EventSourcingRegistration<TRequest, TEntity>(
                typeof(TEvent),
                MapAsync<TEvent>,
                options.ExpectedVersion,
                options.ShouldAppend);
        }

        public static EventSourcingRegistration<TRequest, TEntity> Create<TSource, TEvent>(
            Func<CommandHookContext<TRequest, TEntity>, TSource> source,
            EventSourcingEventOptions<TRequest, TEntity> options)
            where TSource : class
            where TEvent : class
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return new EventSourcingRegistration<TRequest, TEntity>(
                typeof(TEvent),
                (serviceProvider, context, cancellationToken) => MapAsync<TSource, TEvent>(serviceProvider, source(context), cancellationToken),
                options.ExpectedVersion,
                options.ShouldAppend);
        }

        private static async ValueTask<object> MapAsync<TEvent>(
            IServiceProvider serviceProvider,
            CommandHookContext<TRequest, TEntity> context,
            CancellationToken cancellationToken)
            where TEvent : class
        {
            var mapper = serviceProvider.GetRequiredService<IMapperAdapter>();
            var mapContext = new EventSourcingMapContext<TRequest, TEntity>(context.Request, context.Entity);

            return await mapper.MapAsync<EventSourcingMapContext<TRequest, TEntity>, TEvent>(mapContext, cancellationToken);
        }

        private static async ValueTask<object> MapAsync<TSource, TEvent>(
            IServiceProvider serviceProvider,
            TSource source,
            CancellationToken cancellationToken)
            where TSource : class
            where TEvent : class
        {
            var mapper = serviceProvider.GetRequiredService<IMapperAdapter>();

            return await mapper.MapAsync<TSource, TEvent>(source, cancellationToken);
        }
    }
}
