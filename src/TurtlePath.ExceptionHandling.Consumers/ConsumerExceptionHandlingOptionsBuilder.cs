using TurtlePath.ExceptionHandling;

namespace TurtlePath.ExceptionHandling.Consumers
{
    /// <summary>
    /// Provides fluent configuration for message consumer exception handling options.
    /// </summary>
    public sealed class ConsumerExceptionHandlingOptionsBuilder
    {
        private readonly ConsumerExceptionHandlingOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsumerExceptionHandlingOptionsBuilder"/> class.
        /// </summary>
        public ConsumerExceptionHandlingOptionsBuilder(ConsumerExceptionHandlingOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Configures the predicate that determines whether an exception is handled by the boundary.
        /// </summary>
        public ConsumerExceptionHandlingOptionsBuilder HandleWhen(
            Func<Exception, ConsumerExceptionContext, CancellationToken, bool> predicate)
        {
            options.ShouldHandle = predicate ?? throw new ArgumentNullException(nameof(predicate));

            return this;
        }

        /// <summary>
        /// Configures the boundary to rethrow every handled exception after reporting.
        /// </summary>
        public ConsumerExceptionHandlingOptionsBuilder Rethrow()
        {
            options.ShouldRethrow = (_, _) => true;

            return this;
        }

        /// <summary>
        /// Configures the boundary to complete handled exceptions after reporting.
        /// </summary>
        public ConsumerExceptionHandlingOptionsBuilder Complete()
        {
            options.ShouldRethrow = (_, _) => false;

            return this;
        }

        /// <summary>
        /// Configures the predicate that determines whether the original exception is rethrown after reporting.
        /// </summary>
        public ConsumerExceptionHandlingOptionsBuilder RethrowWhen(
            Func<ExceptionDescriptor, ConsumerExceptionContext, bool> predicate)
        {
            options.ShouldRethrow = predicate ?? throw new ArgumentNullException(nameof(predicate));

            return this;
        }
    }
}
