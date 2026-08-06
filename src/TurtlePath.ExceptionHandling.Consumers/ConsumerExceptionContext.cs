namespace TurtlePath.ExceptionHandling.Consumers
{
    /// <summary>
    /// Carries optional message consumer context.
    /// </summary>
    public sealed record ConsumerExceptionContext
    {
        /// <summary>
        /// Gets the message type being consumed.
        /// </summary>
        public Type MessageType { get; init; }

        /// <summary>
        /// Gets the logical message name.
        /// </summary>
        public string MessageName { get; init; }

        /// <summary>
        /// Gets the broker message identifier.
        /// </summary>
        public string MessageId { get; init; }

        /// <summary>
        /// Gets the broker correlation identifier.
        /// </summary>
        public string CorrelationId { get; init; }

        /// <summary>
        /// Gets the delivery attempt count when available.
        /// </summary>
        public int? DeliveryCount { get; init; }

        /// <summary>
        /// Gets optional contextual values.
        /// </summary>
        public IDictionary<string, object> Items { get; init; } = new Dictionary<string, object>();
    }
}
