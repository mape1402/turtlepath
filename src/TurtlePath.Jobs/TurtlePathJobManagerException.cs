namespace TurtlePath.Jobs
{
    /// <summary>
    /// Exception thrown when one or more managed jobs fail.
    /// </summary>
    public sealed class TurtlePathJobManagerException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TurtlePathJobManagerException"/> class.
        /// </summary>
        public TurtlePathJobManagerException(TurtlePathJobManagerResult result)
            : base("One or more TurtlePath jobs failed.")
        {
            Result = result ?? throw new ArgumentNullException(nameof(result));
        }

        /// <summary>
        /// Gets the aggregated job manager result.
        /// </summary>
        public TurtlePathJobManagerResult Result { get; }
    }
}
