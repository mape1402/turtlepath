using Microsoft.Extensions.Options;
using TurtlePath.ExceptionHandling;

namespace TurtlePath.ExceptionHandling.Workers
{
    /// <summary>
    /// Executes background work inside TurtlePath exception handling.
    /// </summary>
    public sealed class BackgroundExceptionBoundary : IBackgroundExceptionBoundary
    {
        private readonly IExceptionHandler exceptionHandler;
        private readonly IBackgroundExceptionReporter reporter;
        private readonly BackgroundExceptionHandlingOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundExceptionBoundary"/> class.
        /// </summary>
        public BackgroundExceptionBoundary(
            IExceptionHandler exceptionHandler,
            IBackgroundExceptionReporter reporter,
            IOptions<BackgroundExceptionHandlingOptions> options)
        {
            this.exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            this.reporter = reporter ?? throw new ArgumentNullException(nameof(reporter));
            this.options = options?.Value ?? new BackgroundExceptionHandlingOptions();
        }

        /// <inheritdoc />
        public async Task RunAsync(
            Func<CancellationToken, Task> work,
            BackgroundExceptionContext context = null,
            CancellationToken cancellationToken = default)
        {
            if (work == null)
                throw new ArgumentNullException(nameof(work));

            try
            {
                await work(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (options.ShouldHandle(exception, cancellationToken))
            {
                var descriptor = exceptionHandler.Handle(exception, CreateHandlingContext(context));
                await reporter.ReportAsync(descriptor, context ?? new BackgroundExceptionContext(), cancellationToken).ConfigureAwait(false);

                if (options.ShouldRethrow(descriptor))
                    throw;
            }
        }

        /// <inheritdoc />
        public async Task<TResult> RunAsync<TResult>(
            Func<CancellationToken, Task<TResult>> work,
            BackgroundExceptionContext context = null,
            CancellationToken cancellationToken = default)
        {
            if (work == null)
                throw new ArgumentNullException(nameof(work));

            try
            {
                return await work(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (options.ShouldHandle(exception, cancellationToken))
            {
                var descriptor = exceptionHandler.Handle(exception, CreateHandlingContext(context));
                await reporter.ReportAsync(descriptor, context ?? new BackgroundExceptionContext(), cancellationToken).ConfigureAwait(false);

                if (options.ShouldRethrow(descriptor))
                    throw;

                return options.CreateDefaultResult<TResult>(descriptor);
            }
        }

        private static ExceptionHandlingContext CreateHandlingContext(BackgroundExceptionContext context)
        {
            return new ExceptionHandlingContext
            {
                TraceIdentifier = context?.TraceIdentifier,
                Items = context?.Items ?? new Dictionary<string, object>()
            };
        }
    }
}
