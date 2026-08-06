namespace TurtlePath.ExceptionHandling
{
    /// <summary>
    /// Carries optional execution context for exception handling.
    /// </summary>
    public sealed class ExceptionHandlingContext
    {
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
