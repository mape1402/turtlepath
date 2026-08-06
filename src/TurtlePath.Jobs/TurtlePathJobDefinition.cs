namespace TurtlePath.Jobs
{
    /// <summary>
    /// Describes a registered one-shot job.
    /// </summary>
    public sealed class TurtlePathJobDefinition
    {
        /// <summary>
        /// Gets the job implementation type.
        /// </summary>
        public Type JobType { get; init; }

        /// <summary>
        /// Gets the logical job name.
        /// </summary>
        public string Name { get; init; }
    }
}
