using TurtlePath.ExceptionHandling;

namespace TurtlePath.ExceptionHandling.Workers
{
    /// <summary>
    /// Provides options for background workload exception handling.
    /// </summary>
    public sealed class BackgroundExceptionHandlingOptions
    {
        /// <summary>
        /// Gets or sets the predicate that determines whether the exception is handled by the boundary.
        /// </summary>
        public Func<Exception, CancellationToken, bool> ShouldHandle { get; set; } =
            (exception, cancellationToken) => exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested;

        /// <summary>
        /// Gets or sets the predicate that determines whether the original exception is rethrown after reporting.
        /// </summary>
        public Func<ExceptionDescriptor, bool> ShouldRethrow { get; set; } = _ => true;

        /// <summary>
        /// Gets or sets the fallback result factory used when a result-returning workload is handled without rethrowing.
        /// </summary>
        public Func<ExceptionDescriptor, object> DefaultResultFactory { get; set; } = _ => default;

        /// <summary>
        /// Creates a typed fallback result.
        /// </summary>
        public TResult CreateDefaultResult<TResult>(ExceptionDescriptor descriptor)
        {
            var result = DefaultResultFactory(descriptor);

            return result is TResult typedResult ? typedResult : default;
        }
    }
}
