namespace TurtlePath.EventSourcing
{
    using Krackend.EventSourcing.Envelopes;
    using Krackend.EventSourcing.Stores;

    /// <summary>
    /// Describes event payloads and envelopes produced by a successful TurtlePath event sourcing append.
    /// </summary>
    /// <typeparam name="TRequest">The command request type.</typeparam>
    /// <typeparam name="TEntity">The entity type affected by the command.</typeparam>
    public sealed class EventSourcingAppendContext<TRequest, TEntity>
        where TRequest : class
        where TEntity : class
    {
        /// <summary>
        /// Gets the command request that produced the appended events.
        /// </summary>
        public required TRequest Request { get; init; }

        /// <summary>
        /// Gets the saved entity that produced the appended events.
        /// </summary>
        public required TEntity Entity { get; init; }

        /// <summary>
        /// Gets the event stream name.
        /// </summary>
        public required string StreamName { get; init; }

        /// <summary>
        /// Gets the event stream id.
        /// </summary>
        public required string StreamId { get; init; }

        /// <summary>
        /// Gets the expected version used for the append.
        /// </summary>
        public required ExpectedVersion ExpectedVersion { get; init; }

        /// <summary>
        /// Gets the event payloads passed to the event store.
        /// </summary>
        public required IReadOnlyCollection<object> Payloads { get; init; }

        /// <summary>
        /// Gets the event envelopes returned by the event store.
        /// </summary>
        public required IReadOnlyCollection<EventEnvelope> Envelopes { get; init; }
    }
}
