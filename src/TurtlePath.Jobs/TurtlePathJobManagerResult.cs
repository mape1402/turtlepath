namespace TurtlePath.Jobs
{
    /// <summary>
    /// Represents the result of a one-shot job manager execution.
    /// </summary>
    public sealed class TurtlePathJobManagerResult
    {
        /// <summary>
        /// Gets whether all jobs succeeded.
        /// </summary>
        public bool Succeeded => Jobs.All(job => job.Succeeded);

        /// <summary>
        /// Gets the job execution results.
        /// </summary>
        public IReadOnlyCollection<TurtlePathJobResult> Jobs { get; init; } = Array.Empty<TurtlePathJobResult>();
    }
}
