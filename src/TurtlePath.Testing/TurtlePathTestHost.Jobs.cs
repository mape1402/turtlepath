namespace TurtlePath.Testing
{
    using TurtlePath.Jobs;

    public sealed partial class TurtlePathTestHost
    {
        /// <summary>
        /// Runs all registered one-shot TurtlePath jobs.
        /// </summary>
        public Task<TurtlePathJobManagerResult> RunJobsAsync(CancellationToken cancellationToken = default)
            => Services.RunTurtlePathJobsAsync(cancellationToken);

        /// <summary>
        /// Runs selected one-shot TurtlePath jobs.
        /// </summary>
        public Task<TurtlePathJobManagerResult> RunJobsAsync(IEnumerable<Type> jobTypes, CancellationToken cancellationToken = default)
            => Services.RunTurtlePathJobsAsync(jobTypes, cancellationToken);
    }
}
