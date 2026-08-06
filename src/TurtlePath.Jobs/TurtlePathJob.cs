namespace TurtlePath.Jobs
{
    /// <summary>
    /// Base class for TurtlePath jobs.
    /// </summary>
    public abstract class TurtlePathJob : ITurtlePathJob
    {
        /// <inheritdoc />
        public abstract Task ExecuteAsync(TurtlePathJobContext context, CancellationToken cancellationToken);
    }
}
