namespace TurtlePath.ExceptionHandling
{
    /// <summary>
    /// Describes an exception in a transport-neutral way.
    /// </summary>
    public sealed class ExceptionDescriptor
    {
        /// <summary>
        /// Gets the original exception.
        /// </summary>
        public Exception Exception { get; init; }

        /// <summary>
        /// Gets the semantic exception kind.
        /// </summary>
        public ExceptionKind Kind { get; init; } = ExceptionKind.Failure;

        /// <summary>
        /// Gets the application-level error code.
        /// </summary>
        public string Code { get; init; } = ExceptionKind.Failure.Value;

        /// <summary>
        /// Gets the extracted messages.
        /// </summary>
        public IReadOnlyCollection<string> Messages { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets additional target-neutral metadata.
        /// </summary>
        public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets the current trace identifier.
        /// </summary>
        public string TraceIdentifier { get; init; }
    }
}
