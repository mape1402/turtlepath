namespace TurtlePath.Testing.Integration
{
    using System.Reflection;
    using global::DataScorpio.Profiles;
    using DynaBee.Testing.DependencyInjection;
    using Krackend.EventSourcing.Testing;
    using Microsoft.Extensions.DependencyInjection;
    using OctoMap;
    using OctoMap.Testing;
    using Pelican.Testing;
    using TurtlePath.Testing;

    /// <summary>
    /// Composes TurtlePath test hosts with Elysium testing adapters.
    /// </summary>
    public static class TurtlePathTestHostBuilderIntegrationExtensions
    {
        /// <summary>
        /// Registers Pelican testing services and handler discovery.
        /// </summary>
        public static TurtlePathTestHostBuilder UsePelicanTesting(
            this TurtlePathTestHostBuilder builder,
            params Assembly[] handlerAssemblies)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder.ConfigureServices(services => services.AddPelicanTestingAdapter(handlerAssemblies));
        }

        /// <summary>
        /// Registers OctoMap testing services and map discovery.
        /// </summary>
        public static TurtlePathTestHostBuilder UseOctoMapTesting(
            this TurtlePathTestHostBuilder builder,
            params Assembly[] mapAssemblies)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder.ConfigureServices(services => services.AddOctoMapTestingAdapter(mapAssemblies));
        }

        /// <summary>
        /// Registers OctoMap testing services and map discovery with custom OctoMap options.
        /// </summary>
        public static TurtlePathTestHostBuilder UseOctoMapTesting(
            this TurtlePathTestHostBuilder builder,
            Action<OctoMapOptions> configureOptions,
            params Assembly[] mapAssemblies)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder.ConfigureServices(services => services.AddOctoMapTestingAdapter(configureOptions, mapAssemblies));
        }

        /// <summary>
        /// Registers Crabalidator testing services and validator discovery.
        /// </summary>
        public static TurtlePathTestHostBuilder UseCrabalidatorTesting(
            this TurtlePathTestHostBuilder builder,
            params Assembly[] validatorAssemblies)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder.ConfigureServices(services => services.AddCrabalidatorTestingAdapter(validatorAssemblies));
        }

        /// <summary>
        /// Registers Pigeon in-memory testing transport and consumer discovery.
        /// </summary>
        public static TurtlePathTestHostBuilder UsePigeonTesting(
            this TurtlePathTestHostBuilder builder,
            params Assembly[] consumerAssemblies)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder.ConfigureServices(services => services.AddPigeonTestingAdapter(consumerAssemblies));
        }

        /// <summary>
        /// Registers Spider testing boundaries and trace services.
        /// </summary>
        public static TurtlePathTestHostBuilder UseSpiderTesting(
            this TurtlePathTestHostBuilder builder,
            params Assembly[] boundaryAssemblies)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder.ConfigureServices(services => services.AddSpiderTesting(boundaryAssemblies));
        }

        /// <summary>
        /// Registers DynaBee testing services.
        /// </summary>
        public static TurtlePathTestHostBuilder UseDynaBeeTesting(this TurtlePathTestHostBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder.ConfigureServices(services => services.AddDynaBeeTesting());
        }

        /// <summary>
        /// Registers Krackend event sourcing testing services and the adapter-friendly in-memory event store.
        /// </summary>
        public static TurtlePathTestHostBuilder UseKrackendTesting(this TurtlePathTestHostBuilder builder)
            => builder.UseKrackendEventSourcingTesting();

        /// <summary>
        /// Registers Krackend event sourcing testing services and the adapter-friendly in-memory event store.
        /// </summary>
        public static TurtlePathTestHostBuilder UseKrackendEventSourcingTesting(this TurtlePathTestHostBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder.ConfigureServices(services => services.AddKrackendEventSourcingTestingAdapter());
        }

        /// <summary>
        /// Registers DataScorpio in-memory testing services.
        /// </summary>
        public static TurtlePathTestHostBuilder UseDataScorpioTesting(
            this TurtlePathTestHostBuilder builder,
            Action<QueryProfileRegistryBuilder> configureProfiles)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder.ConfigureServices(services => services.AddDataScorpioTesting(configureProfiles));
        }

        /// <summary>
        /// Registers DataScorpio SQLite-backed testing services.
        /// </summary>
        public static TurtlePathTestHostBuilder UseDataScorpioSqliteTesting(
            this TurtlePathTestHostBuilder builder,
            Action<QueryProfileRegistryBuilder> configureProfiles)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder.ConfigureServices(services => services.AddDataScorpioSqliteTesting(configureProfiles));
        }
    }
}
