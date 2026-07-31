namespace TurtlePath
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Represents a TurtlePath registration pipeline.
    /// </summary>
    public interface ITurtlePathBuilder
    {
        /// <summary>
        /// Gets the service collection being configured.
        /// </summary>
        IServiceCollection Services { get; }
    }
}
