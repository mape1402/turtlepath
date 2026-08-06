using Microsoft.Extensions.Logging;
using TurtlePath.ExceptionHandling;

namespace TurtlePath.ExceptionHandling.Workers
{
    /// <summary>
    /// Reports background exceptions through Microsoft.Extensions.Logging.
    /// </summary>
    public sealed class LoggingBackgroundExceptionReporter : IBackgroundExceptionReporter
    {
        private readonly ILogger<LoggingBackgroundExceptionReporter> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingBackgroundExceptionReporter"/> class.
        /// </summary>
        public LoggingBackgroundExceptionReporter(ILogger<LoggingBackgroundExceptionReporter> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public Task ReportAsync(
            ExceptionDescriptor descriptor,
            BackgroundExceptionContext context = null,
            CancellationToken cancellationToken = default)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            logger.LogError(
                descriptor.Exception,
                "Background workload {Workload} failed with kind {Kind}, code {Code}, trace {TraceIdentifier}: {Messages}",
                context?.Workload,
                descriptor.Kind.Value,
                descriptor.Code,
                descriptor.TraceIdentifier,
                string.Join(" | ", descriptor.Messages));

            return Task.CompletedTask;
        }
    }
}
