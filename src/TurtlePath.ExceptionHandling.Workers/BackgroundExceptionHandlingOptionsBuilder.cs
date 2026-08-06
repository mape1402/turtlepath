using TurtlePath.ExceptionHandling;

namespace TurtlePath.ExceptionHandling.Workers
{
    /// <summary>
    /// Provides fluent configuration for background exception handling options.
    /// </summary>
    public sealed class BackgroundExceptionHandlingOptionsBuilder
    {
        private readonly BackgroundExceptionHandlingOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundExceptionHandlingOptionsBuilder"/> class.
        /// </summary>
        public BackgroundExceptionHandlingOptionsBuilder(BackgroundExceptionHandlingOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Configures the predicate that determines whether an exception is handled by the boundary.
        /// </summary>
        public BackgroundExceptionHandlingOptionsBuilder HandleWhen(Func<Exception, CancellationToken, bool> predicate)
        {
            options.ShouldHandle = predicate ?? throw new ArgumentNullException(nameof(predicate));

            return this;
        }

        /// <summary>
        /// Configures the boundary to rethrow every handled exception after reporting.
        /// </summary>
        public BackgroundExceptionHandlingOptionsBuilder Rethrow()
        {
            options.ShouldRethrow = _ => true;

            return this;
        }

        /// <summary>
        /// Configures the boundary to complete handled exceptions after reporting.
        /// </summary>
        public BackgroundExceptionHandlingOptionsBuilder Complete()
        {
            options.ShouldRethrow = _ => false;

            return this;
        }

        /// <summary>
        /// Configures the predicate that determines whether the original exception is rethrown after reporting.
        /// </summary>
        public BackgroundExceptionHandlingOptionsBuilder RethrowWhen(Func<ExceptionDescriptor, bool> predicate)
        {
            options.ShouldRethrow = predicate ?? throw new ArgumentNullException(nameof(predicate));

            return this;
        }

        /// <summary>
        /// Configures the fallback result used when a handled exception is completed.
        /// </summary>
        public BackgroundExceptionHandlingOptionsBuilder Return(Func<ExceptionDescriptor, object> resultFactory)
        {
            options.DefaultResultFactory = resultFactory ?? throw new ArgumentNullException(nameof(resultFactory));

            return this;
        }
    }
}
