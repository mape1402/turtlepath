namespace TurtlePath.EventSourcing
{
    using Krackend.EventSourcing.Stores;
    using Krackend.EventSourcing.Streams;
    using TurtlePath.EventSourcing.Internal;
    using TurtlePath.Hooks;

    /// <summary>
    /// Appends configured event payloads after a TurtlePath command handler saves an entity.
    /// </summary>
    /// <typeparam name="TRequest">The command request type.</typeparam>
    /// <typeparam name="TEntity">The entity type affected by the command.</typeparam>
    internal sealed class EventSourcingAfterSaveHook<TRequest, TEntity>
        : IAfterSaveHook<TRequest, TEntity>, IOrderedHook
        where TRequest : class
        where TEntity : class
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ICommandStreamResolver<TRequest> streamResolver;
        private readonly IEventStore eventStore;
        private readonly EventSourcingRegistrationRegistry registry;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSourcingAfterSaveHook{TRequest, TEntity}"/> class.
        /// </summary>
        public EventSourcingAfterSaveHook(
            IServiceProvider serviceProvider,
            ICommandStreamResolver<TRequest> streamResolver,
            IEventStore eventStore,
            EventSourcingRegistrationRegistry registry)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.streamResolver = streamResolver ?? throw new ArgumentNullException(nameof(streamResolver));
            this.eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        /// <inheritdoc />
        public int Order => int.MaxValue;

        /// <inheritdoc />
        public async ValueTask AfterSaveAsync(
            CommandHookContext<TRequest, TEntity> context,
            CancellationToken cancellationToken = default)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.Entity == null)
                throw new InvalidOperationException("Entity must be available after save to append event sourcing payloads.");

            var registrations = registry.Get<TRequest, TEntity>();

            if (registrations.Count == 0)
                return;

            var stream = registry.GetStream<TRequest, TEntity>()?.Resolve(context)
                ?? streamResolver.Resolve(context.Request);
            var pending = new List<PendingEvent>();

            foreach (var registration in registrations)
            {
                if (!registration.ShouldAppend(context))
                    continue;

                var payload = await registration.Map(serviceProvider, context, cancellationToken);

                if (payload == null)
                    continue;

                pending.Add(new PendingEvent(registration.ExpectedVersion(context), payload));
            }

            foreach (var batch in CreateBatches(pending))
            {
                await eventStore.AppendAsync(
                    stream.Name,
                    stream.Id,
                    batch.ExpectedVersion,
                    batch.Events,
                    cancellationToken);
            }
        }

        private static IEnumerable<EventBatch> CreateBatches(IReadOnlyCollection<PendingEvent> events)
        {
            if (events.Count == 0)
                yield break;

            ExpectedVersion? currentExpectedVersion = null;
            var currentEvents = new List<object>();

            foreach (var item in events)
            {
                if (currentExpectedVersion.HasValue && currentExpectedVersion.Value != item.ExpectedVersion)
                {
                    yield return new EventBatch(currentExpectedVersion.Value, currentEvents.ToArray());
                    currentEvents.Clear();
                }

                currentExpectedVersion = item.ExpectedVersion;
                currentEvents.Add(item.Payload);
            }

            if (currentExpectedVersion.HasValue && currentEvents.Count > 0)
                yield return new EventBatch(currentExpectedVersion.Value, currentEvents.ToArray());
        }

        private readonly record struct PendingEvent(ExpectedVersion ExpectedVersion, object Payload);

        private readonly record struct EventBatch(ExpectedVersion ExpectedVersion, IReadOnlyCollection<object> Events);
    }
}
