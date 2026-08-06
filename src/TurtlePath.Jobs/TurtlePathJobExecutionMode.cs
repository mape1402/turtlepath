namespace TurtlePath.Jobs
{
    /// <summary>
    /// Defines how one-shot jobs are executed by the manager.
    /// </summary>
    public enum TurtlePathJobExecutionMode
    {
        /// <summary>
        /// Executes jobs one after another.
        /// </summary>
        Sequential,

        /// <summary>
        /// Executes jobs in parallel.
        /// </summary>
        Parallel
    }
}
