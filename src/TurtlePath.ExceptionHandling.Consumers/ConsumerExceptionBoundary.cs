using Microsoft.Extensions.Options;
using TurtlePath.ExceptionHandling;

namespace TurtlePath.ExceptionHandling.Consumers
{
    /// <summary>
    /// Executes message consumers inside TurtlePath exception handling.
    /// </summary>
    public sealed class ConsumerExceptionBoundary : IConsumerExceptionBoundary
    {
        private readonly IExceptionHandler exceptionHandler;
        private readonly IConsumerExceptionReporter reporter;
        private readonly ConsumerExceptionHandlingOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsumerExceptionBoundary"/> class.
        /// </summary>
        public ConsumerExceptionBoundary(
            IExceptionHandler exceptionHandler,
            IConsumerExceptionReporter reporter,
            IOptions<ConsumerExceptionHandlingOptions> options)
        {
            this.exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            this.reporter = reporter ?? throw new ArgumentNullException(nameof(reporter));
            this.options = options?.Value ?? new ConsumerExceptionHandlingOptions();
        }

        /// <inheritdoc />
        public Task RunAsync<TMessage>(
            TMessage message,
            Func<TMessage, CancellationToken, Task> consume,
            ConsumerExceptionContext context = null,
            CancellationToken cancellationToken = default)
        {
            if (consume == null)
                throw new ArgumentNullException(nameof(consume));

            return RunAsync(
                token => consume(message, token),
                EnrichContext<TMessage>(context),
                cancellationToken);
        }

        /// <inheritdoc />
        public async Task RunAsync(
            Func<CancellationToken, Task> consume,
            ConsumerExceptionContext context = null,
            CancellationToken cancellationToken = default)
        {
            if (consume == null)
                throw new ArgumentNullException(nameof(consume));

            try
            {
                await consume(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (options.ShouldHandle(exception, context, cancellationToken))
            {
                var descriptor = exceptionHandler.Handle(exception, CreateHandlingContext(context));
                await reporter.ReportAsync(descriptor, context ?? new ConsumerExceptionContext(), cancellationToken).ConfigureAwait(false);

                if (options.ShouldRethrow(descriptor, context))
                    throw;
            }
        }

        private static ConsumerExceptionContext EnrichContext<TMessage>(ConsumerExceptionContext context)
        {
            context ??= new ConsumerExceptionContext();

            return context with
            {
                MessageType = context.MessageType ?? typeof(TMessage),
                MessageName = context.MessageName ?? typeof(TMessage).Name
            };
        }

        private static ExceptionHandlingContext CreateHandlingContext(ConsumerExceptionContext context)
        {
            return new ExceptionHandlingContext
            {
                TraceIdentifier = context?.CorrelationId ?? context?.MessageId,
                Items = context?.Items ?? new Dictionary<string, object>()
            };
        }
    }
}
