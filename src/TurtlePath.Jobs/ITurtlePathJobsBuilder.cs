using Microsoft.Extensions.DependencyInjection;

namespace TurtlePath.Jobs
{
    /// <summary>
    /// Provides a fluent builder for TurtlePath jobs.
    /// </summary>
    public interface ITurtlePathJobsBuilder
    {
        /// <summary>
        /// Gets the service collection.
        /// </summary>
        IServiceCollection Services { get; }
    }
}
