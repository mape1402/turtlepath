using Microsoft.Extensions.Logging;
using TurtlePath.ExceptionHandling;

namespace TurtlePath.ExceptionHandling.Consumers
{
    /// <summary>
    /// Reports consumer exceptions through Microsoft.Extensions.Logging.
    /// </summary>
    public sealed class LoggingConsumerExceptionReporter : IConsumerExceptionReporter
    {
        private readonly ILogger<LoggingConsumerExceptionReporter> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingConsumerExceptionReporter"/> class.
        /// </summary>
        public LoggingConsumerExceptionReporter(ILogger<LoggingConsumerExceptionReporter> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public Task ReportAsync(
            ExceptionDescriptor descriptor,
            ConsumerExceptionContext context = null,
            CancellationToken cancellationToken = default)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            logger.LogError(
                descriptor.Exception,
                "Consumer {MessageName} failed with kind {Kind}, code {Code}, message id {MessageId}, correlation id {CorrelationId}, delivery count {DeliveryCount}: {Messages}",
                context?.MessageName,
                descriptor.Kind.Value,
                descriptor.Code,
                context?.MessageId,
                context?.CorrelationId,
                context?.DeliveryCount,
                string.Join(" | ", descriptor.Messages));

            return Task.CompletedTask;
        }
    }
}
