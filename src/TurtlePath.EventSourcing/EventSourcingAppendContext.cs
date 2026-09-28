namespace TurtlePath.EventSourcing
{
    using Krackend.EventSourcing.Envelopes;
    using Krackend.EventSourcing.Stores;

    /// <summary>
    /// Describes event payloads and envelopes produced by a successful TurtlePath event sourcing append.
    /// </summary>
    public sealed class EventSourcingAppendContext
    {
        /// <summary>
        /// Gets the command request that produced the appended events.
        /// </summary>
        public required object Request { get; init; }

        /// <summary>
        /// Gets the saved entity that produced the appended events.
        /// </summary>
        public required object Entity { get; init; }

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
