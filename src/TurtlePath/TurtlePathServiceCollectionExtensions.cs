namespace Microsoft.Extensions.DependencyInjection
{
    using System.Reflection;
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
        public static IServiceCollection AddTurtlePath(this IServiceCollection services, params Assembly[] hookAssemblies)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (hookAssemblies?.Length > 0)
                services.AddHandlerHooksFromAssemblies(hookAssemblies);

            return services;
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
        public static IServiceCollection AddTurtlePath<TTargetType, TDbType>(
            this IServiceCollection services,
            Action<CIdConfiguration<TTargetType, TDbType>> configureIdentifier,
            params Assembly[] hookAssemblies)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.UseCId(configureIdentifier);
            services.AddTurtlePath(hookAssemblies);

            return services;
        }
    }
}
