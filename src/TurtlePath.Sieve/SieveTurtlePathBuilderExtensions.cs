namespace Microsoft.Extensions.DependencyInjection
{
    using global::Sieve.Services;
    using TurtlePath.Persistence;
    using TurtlePath.Sieve;
    using TurtlePath;

    /// <summary>
    /// Provides Sieve registration helpers for TurtlePath.
    /// </summary>
    public static class SieveTurtlePathBuilderExtensions
    {
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

        private static IServiceCollection AddTurtlePathSieve(this IServiceCollection services)
        {
            services.AddSingleton<ISieveProcessor, SieveProcessor>();
            services.AddSingleton<IStorageCriteriaApplier, SieveStorageCriteriaApplier>();

            return services;
        }
    }
}
