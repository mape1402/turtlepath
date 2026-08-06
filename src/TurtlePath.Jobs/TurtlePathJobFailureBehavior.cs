namespace TurtlePath.Jobs
{
    /// <summary>
    /// Defines what a job runner does when a job exhausts its retries.
    /// </summary>
    public enum TurtlePathJobFailureBehavior
    {
        /// <summary>
        /// Rethrows the exception.
        /// </summary>
        Rethrow,

        /// <summary>
        /// Records the failure and keeps processing.
        /// </summary>
        Continue,

        /// <summary>
        /// Requests the host to stop.
        /// </summary>
        StopHost
    }
}
