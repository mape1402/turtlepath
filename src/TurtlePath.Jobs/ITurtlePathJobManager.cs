namespace TurtlePath.Jobs
{
    /// <summary>
    /// Executes registered one-shot jobs.
    /// </summary>
    public interface ITurtlePathJobManager
    {
        /// <summary>
        /// Executes registered jobs.
        /// </summary>
        Task<TurtlePathJobManagerResult> RunAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the specified registered job types.
        /// </summary>
        Task<TurtlePathJobManagerResult> RunAsync(IEnumerable<Type> jobTypes, CancellationToken cancellationToken = default);
    }
}
