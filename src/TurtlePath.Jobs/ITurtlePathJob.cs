namespace TurtlePath.Jobs
{
    /// <summary>
    /// Represents a TurtlePath executable job.
    /// </summary>
    public interface ITurtlePathJob
    {
        /// <summary>
        /// Executes the job.
        /// </summary>
        Task ExecuteAsync(TurtlePathJobContext context, CancellationToken cancellationToken);
    }
}
