namespace Microsoft.Extensions.DependencyInjection
{
    using global::Sieve.Services;
    using TurtlePath.Persistence;
    using TurtlePath.Sieve;

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
            services.AddSingleton<ISieveProcessor, SieveProcessor>();
            services.AddSingleton<IStorageCriteriaApplier, SieveStorageCriteriaApplier>();

            return services;
        }
    }
}
