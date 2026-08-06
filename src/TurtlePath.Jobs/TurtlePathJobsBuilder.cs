using Microsoft.Extensions.DependencyInjection;

namespace TurtlePath.Jobs
{
    internal sealed class TurtlePathJobsBuilder : ITurtlePathJobsBuilder
    {
        public TurtlePathJobsBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IServiceCollection Services { get; }
    }
}
