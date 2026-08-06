namespace TurtlePath.Jobs
{
    /// <summary>
    /// Provides options for recurring cron-style jobs.
    /// </summary>
    public sealed class TurtlePathCronJobOptions : TurtlePathJobExecutionOptions
    {
        /// <summary>
        /// Gets or sets the interval between successful or handled executions.
        /// </summary>
        public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Gets or sets whether the job runs immediately when the hosted service starts.
        /// </summary>
        public bool RunOnStart { get; set; }

        /// <summary>
        /// Configures the job to run every specified interval.
        /// </summary>
        public TurtlePathCronJobOptions Every(TimeSpan interval)
        {
            if (interval <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(interval));

            Interval = interval;

            return this;
        }

        /// <summary>
        /// Configures the job to run every specified number of seconds.
        /// </summary>
        public TurtlePathCronJobOptions EverySeconds(int seconds) => Every(TimeSpan.FromSeconds(seconds));

        /// <summary>
        /// Configures the job to run every specified number of minutes.
        /// </summary>
        public TurtlePathCronJobOptions EveryMinutes(int minutes) => Every(TimeSpan.FromMinutes(minutes));

        /// <summary>
        /// Configures the job to run every specified number of hours.
        /// </summary>
        public TurtlePathCronJobOptions EveryHours(int hours) => Every(TimeSpan.FromHours(hours));
    }
}
