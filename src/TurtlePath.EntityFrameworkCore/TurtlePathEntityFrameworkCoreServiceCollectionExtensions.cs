namespace Microsoft.Extensions.DependencyInjection
{
    using TurtlePath.EntityFrameworkCore;

    /// <summary>
    /// Provides registration helpers for TurtlePath Entity Framework Core integration.
    /// </summary>
    public static class TurtlePathEntityFrameworkCoreServiceCollectionExtensions
    {
        /// <summary>
        /// Registers TurtlePath Entity Framework Core options.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configure">The options configuration callback.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddTurtlePathEntityFrameworkCore(
            this IServiceCollection services,
            Func<TurtlePathDbContextOptions, TurtlePathDbContextOptions>? configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var options = configure?.Invoke(TurtlePathDbContextOptions.Default) ?? TurtlePathDbContextOptions.Default;

            services.AddSingleton(options);

            return services;
        }
    }
}
