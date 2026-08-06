namespace TurtlePath.Jobs
{
    /// <summary>
    /// Provides execution options shared by one-shot and recurring jobs.
    /// </summary>
    public class TurtlePathJobExecutionOptions
    {
        /// <summary>
        /// Gets or sets the number of retries after the first failed attempt.
        /// </summary>
        public int Retries { get; set; }

        /// <summary>
        /// Gets or sets the delay between retries.
        /// </summary>
        public TimeSpan RetryDelay { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Gets or sets what happens after a job exhausts its retries.
        /// </summary>
        public TurtlePathJobFailureBehavior FailureBehavior { get; set; } = TurtlePathJobFailureBehavior.Rethrow;
    }
}
