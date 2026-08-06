namespace TurtlePath.Jobs
{
    /// <summary>
    /// Describes a registered recurring cron-style job.
    /// </summary>
    public sealed class TurtlePathCronJobDefinition
    {
        /// <summary>
        /// Gets the job implementation type.
        /// </summary>
        public Type JobType { get; init; }

        /// <summary>
        /// Gets the logical job name.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the recurring execution options.
        /// </summary>
        public TurtlePathCronJobOptions Options { get; init; } = new();
    }
}
