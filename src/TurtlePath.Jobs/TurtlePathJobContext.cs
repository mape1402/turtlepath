namespace TurtlePath.Jobs
{
    /// <summary>
    /// Carries execution context for a TurtlePath job.
    /// </summary>
    public sealed class TurtlePathJobContext
    {
        /// <summary>
        /// Gets the job name.
        /// </summary>
        public string JobName { get; init; }

        /// <summary>
        /// Gets the unique execution identifier.
        /// </summary>
        public string ExecutionId { get; init; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Gets the UTC start date for the execution.
        /// </summary>
        public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets the current attempt number.
        /// </summary>
        public int Attempt { get; init; } = 1;

        /// <summary>
        /// Gets optional contextual values.
        /// </summary>
        public IDictionary<string, object> Items { get; init; } = new Dictionary<string, object>();

        /// <summary>
        /// Creates a copy of the context for a specific attempt.
        /// </summary>
        public TurtlePathJobContext ForAttempt(int attempt)
        {
            return new TurtlePathJobContext
            {
                JobName = JobName,
                ExecutionId = ExecutionId,
                StartedAt = StartedAt,
                Attempt = attempt,
                Items = Items
            };
        }
    }
}
