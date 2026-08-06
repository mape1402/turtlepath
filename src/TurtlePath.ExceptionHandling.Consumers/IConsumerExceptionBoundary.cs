namespace TurtlePath.ExceptionHandling.Consumers
{
    /// <summary>
    /// Executes message consumers inside exception handling.
    /// </summary>
    public interface IConsumerExceptionBoundary
    {
        /// <summary>
        /// Executes a typed message consumer and handles exceptions according to the configured policy.
        /// </summary>
        Task RunAsync<TMessage>(
            TMessage message,
            Func<TMessage, CancellationToken, Task> consume,
            ConsumerExceptionContext context = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a message consumer and handles exceptions according to the configured policy.
        /// </summary>
        Task RunAsync(
            Func<CancellationToken, Task> consume,
            ConsumerExceptionContext context = null,
            CancellationToken cancellationToken = default);
    }
}
