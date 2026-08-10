namespace Microsoft.Extensions.DependencyInjection
{
    using global::DataScorpio.Profiles;
    using TurtlePath;
    using TurtlePath.DataScorpio;
    using TurtlePath.Persistence;

    /// <summary>
    /// Provides DataScorpio registration helpers for TurtlePath.
    /// </summary>
    public static class DataScorpioTurtlePathBuilderExtensions
    {
        /// <summary>
        /// Registers DataScorpio filtering and sorting support on the current TurtlePath pipeline.
        /// </summary>
        /// <param name="builder">The TurtlePath builder.</param>
        /// <param name="configureProfiles">The DataScorpio profile configuration.</param>
        /// <returns>The same TurtlePath builder.</returns>
        public static ITurtlePathBuilder UseDataScorpio(
            this ITurtlePathBuilder builder,
            Action<QueryProfileRegistryBuilder> configureProfiles)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddTurtlePathDataScorpio(configureProfiles);

            return builder;
        }

        /// <summary>
        /// Registers DataScorpio filtering and sorting support for TurtlePath storage adapters.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureProfiles">The DataScorpio profile configuration.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddTurtlePathDataScorpio(
            this IServiceCollection services,
            Action<QueryProfileRegistryBuilder> configureProfiles)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddDataScorpioSieveCompatibility(configureProfiles);
            services.AddSingleton<IStorageCriteriaApplier, DataScorpioStorageCriteriaApplier>();

            return services;
        }
    }
}
