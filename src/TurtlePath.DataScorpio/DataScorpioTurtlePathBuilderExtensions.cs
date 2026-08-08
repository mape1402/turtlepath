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
        /// Registers DataScorpio criteria support on the current TurtlePath pipeline.
        /// </summary>
        /// <param name="builder">The TurtlePath builder.</param>
        /// <param name="configureProfiles">The DataScorpio profile registry configuration.</param>
        /// <returns>The same TurtlePath builder.</returns>
        public static ITurtlePathBuilder UseDataScorpio(
            this ITurtlePathBuilder builder,
            Action<QueryProfileRegistryBuilder> configureProfiles)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddDataScorpioSieveCompatibility(configureProfiles);
            builder.Services.AddSingleton<IStorageCriteriaApplier, DataScorpioStorageCriteriaApplier>();

            return builder;
        }
    }
}
