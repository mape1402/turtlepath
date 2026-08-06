namespace TurtlePath.ExceptionHandling
{
    /// <summary>
    /// Provides configuration for transport-neutral exception handling.
    /// </summary>
    public sealed class ExceptionHandlingOptions
    {
        /// <summary>
        /// Gets the configured exception handling rules by exact exception type.
        /// </summary>
        internal Dictionary<Type, ExceptionHandlingRule> Rules { get; } = new();

        /// <summary>
        /// Gets or sets the fallback exception kind.
        /// </summary>
        public ExceptionKind FallbackKind { get; set; } = ExceptionKind.Failure;

        /// <summary>
        /// Gets or sets the fallback application error code.
        /// </summary>
        public string FallbackCode { get; set; } = ExceptionKind.Failure.Value;

        /// <summary>
        /// Gets or sets the fallback message factory.
        /// </summary>
        public Func<Exception, IEnumerable<string>> FallbackMessages { get; set; } = exception => new[] { exception.Message };

        /// <summary>
        /// Gets or sets the fallback metadata factory.
        /// </summary>
        public Func<Exception, IReadOnlyDictionary<string, object>> FallbackMetadata { get; set; } =
            _ => new Dictionary<string, object>();
    }
}
