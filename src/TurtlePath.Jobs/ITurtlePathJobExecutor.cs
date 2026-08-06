namespace TurtlePath.Jobs
{
    /// <summary>
    /// Executes a single TurtlePath job using retries and exception handling.
    /// </summary>
    public interface ITurtlePathJobExecutor
    {
        /// <summary>
        /// Executes the specified job type.
        /// </summary>
        Task<TurtlePathJobResult> ExecuteAsync(
            Type jobType,
            string jobName,
            TurtlePathJobExecutionOptions options,
            CancellationToken cancellationToken = default);
    }
}
