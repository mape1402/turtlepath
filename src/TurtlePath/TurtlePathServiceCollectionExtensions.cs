namespace Microsoft.Extensions.DependencyInjection
{
    using System.Reflection;
    using TurtlePath;
    using TurtlePath.Domain.Identifier;
    using TurtlePath.Hooks;

    /// <summary>
    /// Provides registration helpers for the main TurtlePath package.
    /// </summary>
    public static class TurtlePathServiceCollectionExtensions
    {
        /// <summary>
        /// Registers TurtlePath hook discovery for the supplied assemblies.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="hookAssemblies">Assemblies that contain TurtlePath hooks.</param>
        /// <returns>The same service collection.</returns>
        public static ITurtlePathBuilder AddTurtlePath(this IServiceCollection services, params Assembly[] hookAssemblies)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (hookAssemblies?.Length > 0)
                services.AddHandlerHooksFromAssemblies(hookAssemblies);

            return new TurtlePathBuilder(services);
        }

        /// <summary>
        /// Registers TurtlePath identifier configuration and hook discovery for the supplied assemblies.
        /// </summary>
        /// <typeparam name="TTargetType">The domain identifier value type.</typeparam>
        /// <typeparam name="TDbType">The database identifier value type.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="configureIdentifier">The CId configuration callback.</param>
        /// <param name="hookAssemblies">Assemblies that contain TurtlePath hooks.</param>
        /// <returns>The same service collection.</returns>
        public static ITurtlePathBuilder AddTurtlePath<TTargetType, TDbType>(
            this IServiceCollection services,
            Action<CIdConfiguration<TTargetType, TDbType>> configureIdentifier,
            params Assembly[] hookAssemblies)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var builder = services.AddTurtlePath(hookAssemblies);
            builder.UseCId(configureIdentifier);

            return builder;
        }

        /// <summary>
        /// Configures TurtlePath identifiers on the current registration pipeline.
        /// </summary>
        /// <typeparam name="TTargetType">The domain identifier value type.</typeparam>
        /// <typeparam name="TDbType">The database identifier value type.</typeparam>
        /// <param name="builder">The TurtlePath builder.</param>
        /// <param name="configureIdentifier">The CId configuration callback.</param>
        /// <returns>The same TurtlePath builder.</returns>
        public static ITurtlePathBuilder UseCId<TTargetType, TDbType>(
            this ITurtlePathBuilder builder,
            Action<CIdConfiguration<TTargetType, TDbType>> configureIdentifier)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.UseCId(configureIdentifier);

            return builder;
        }
    }
}
