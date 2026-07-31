namespace TurtlePath
{
    using Microsoft.Extensions.DependencyInjection;

    internal sealed class TurtlePathBuilder : ITurtlePathBuilder
    {
        public TurtlePathBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IServiceCollection Services { get; }
    }
}
