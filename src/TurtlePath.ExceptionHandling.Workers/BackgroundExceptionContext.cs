namespace TurtlePath.ExceptionHandling.Workers
{
    /// <summary>
    /// Carries optional background workload context.
    /// </summary>
    public sealed class BackgroundExceptionContext
    {
        /// <summary>
        /// Gets the logical workload name.
        /// </summary>
        public string Workload { get; init; }

        /// <summary>
        /// Gets the current trace identifier.
        /// </summary>
        public string TraceIdentifier { get; init; }

        /// <summary>
        /// Gets optional contextual values.
        /// </summary>
        public IDictionary<string, object> Items { get; init; } = new Dictionary<string, object>();
    }
}
