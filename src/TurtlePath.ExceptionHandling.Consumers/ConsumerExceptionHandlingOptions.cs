using TurtlePath.ExceptionHandling;

namespace TurtlePath.ExceptionHandling.Consumers
{
    /// <summary>
    /// Provides options for message consumer exception handling.
    /// </summary>
    public sealed class ConsumerExceptionHandlingOptions
    {
        /// <summary>
        /// Gets or sets the predicate that determines whether the exception is handled by the boundary.
        /// </summary>
        public Func<Exception, ConsumerExceptionContext, CancellationToken, bool> ShouldHandle { get; set; } =
            (exception, _, cancellationToken) => exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested;

        /// <summary>
        /// Gets or sets the predicate that determines whether the original exception is rethrown after reporting.
        /// </summary>
        public Func<ExceptionDescriptor, ConsumerExceptionContext, bool> ShouldRethrow { get; set; } = (_, _) => true;
    }
}
