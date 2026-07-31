namespace Microsoft.Extensions.DependencyInjection
{
    using global::Sieve.Services;
    using TurtlePath.Persistence;
    using TurtlePath.Sieve;
    using TurtlePath;

    /// <summary>
    /// Provides Sieve registration helpers for TurtlePath.
    /// </summary>
    public static class SieveServiceCollectionExtensions
    {
        /// <summary>
        /// Registers Sieve criteria support for TurtlePath storage abstractions.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddTurtlePathSieve(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton<ISieveProcessor, SieveProcessor>();
            services.AddSingleton<IStorageCriteriaApplier, SieveStorageCriteriaApplier>();

            return services;
        }

        /// <summary>
        /// Registers Sieve criteria support on the current TurtlePath pipeline.
        /// </summary>
        /// <param name="builder">The TurtlePath builder.</param>
        /// <returns>The same TurtlePath builder.</returns>
        public static ITurtlePathBuilder UseSieve(this ITurtlePathBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddTurtlePathSieve();

            return builder;
        }
    }
}
