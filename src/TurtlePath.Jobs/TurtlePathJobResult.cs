using TurtlePath.ExceptionHandling;

namespace TurtlePath.Jobs
{
    /// <summary>
    /// Represents the result of a job execution.
    /// </summary>
    public sealed class TurtlePathJobResult
    {
        /// <summary>
        /// Gets the job name.
        /// </summary>
        public string JobName { get; init; }

        /// <summary>
        /// Gets the job type.
        /// </summary>
        public Type JobType { get; init; }

        /// <summary>
        /// Gets whether the job execution succeeded.
        /// </summary>
        public bool Succeeded { get; init; }

        /// <summary>
        /// Gets the number of attempts used by this execution.
        /// </summary>
        public int Attempts { get; init; }

        /// <summary>
        /// Gets the execution duration.
        /// </summary>
        public TimeSpan Duration { get; init; }

        /// <summary>
        /// Gets the exception descriptor when execution failed.
        /// </summary>
        public ExceptionDescriptor Exception { get; init; }
    }
}
