namespace TurtlePath.Jobs
{
    /// <summary>
    /// Provides options for one-shot job manager executions.
    /// </summary>
    public sealed class TurtlePathJobManagerOptions : TurtlePathJobExecutionOptions
    {
        /// <summary>
        /// Gets or sets the default one-shot execution mode.
        /// </summary>
        public TurtlePathJobExecutionMode ExecutionMode { get; set; } = TurtlePathJobExecutionMode.Parallel;

        /// <summary>
        /// Gets or sets the maximum number of one-shot jobs to run concurrently.
        /// </summary>
        public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;
    }
}
