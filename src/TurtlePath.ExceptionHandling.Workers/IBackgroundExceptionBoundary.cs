namespace TurtlePath.ExceptionHandling.Workers
{
    /// <summary>
    /// Executes background workloads inside exception handling.
    /// </summary>
    public interface IBackgroundExceptionBoundary
    {
        /// <summary>
        /// Executes work and handles exceptions according to the configured policy.
        /// </summary>
        Task RunAsync(
            Func<CancellationToken, Task> work,
            BackgroundExceptionContext context = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes work that returns a result and handles exceptions according to the configured policy.
        /// </summary>
        Task<TResult> RunAsync<TResult>(
            Func<CancellationToken, Task<TResult>> work,
            BackgroundExceptionContext context = null,
            CancellationToken cancellationToken = default);
    }
}
