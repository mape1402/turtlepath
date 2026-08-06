using Microsoft.Extensions.Options;

namespace TurtlePath.ExceptionHandling
{
    /// <summary>
    /// Resolves exceptions into transport-neutral descriptors.
    /// </summary>
    public sealed class DefaultExceptionHandler : IExceptionHandler
    {
        private readonly ExceptionHandlingOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultExceptionHandler"/> class.
        /// </summary>
        /// <param name="options">The exception handling options.</param>
        public DefaultExceptionHandler(IOptions<ExceptionHandlingOptions> options)
        {
            this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc />
        public ExceptionDescriptor Handle(Exception exception, ExceptionHandlingContext context = null)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            var rule = FindRule(exception);
            var kind = rule?.KindFactory(exception) ?? options.FallbackKind;
            var code = rule?.CodeFactory(exception) ?? options.FallbackCode;
            var messages = rule?.MessageFactory(exception) ?? options.FallbackMessages(exception);
            var metadata = rule?.MetadataFactory(exception) ?? options.FallbackMetadata(exception);

            return new ExceptionDescriptor
            {
                Exception = exception,
                Kind = kind,
                Code = string.IsNullOrWhiteSpace(code) ? kind.Value : code,
                Messages = messages.Where(message => !string.IsNullOrWhiteSpace(message)).ToArray(),
                Metadata = metadata ?? new Dictionary<string, object>(),
                TraceIdentifier = context?.TraceIdentifier
            };
        }

        private ExceptionHandlingRule FindRule(Exception exception)
            => options.Rules.TryGetValue(exception.GetType(), out var rule) ? rule : null;
    }
}
