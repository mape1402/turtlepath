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

            AddCIdDefinition(builder.Services, CIdDefinition.DefaultContext, null, CIdDefinition.DefaultPropertyName, configureIdentifier);

            return builder;
        }

        /// <summary>
        /// Configures TurtlePath identifiers for a specific entity on the current registration pipeline.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <typeparam name="TTargetType">The domain identifier value type.</typeparam>
        /// <typeparam name="TDbType">The database identifier value type.</typeparam>
        /// <param name="builder">The TurtlePath builder.</param>
        /// <param name="configureIdentifier">The CId configuration callback.</param>
        /// <param name="propertyName">The identifier property name.</param>
        /// <returns>The same TurtlePath builder.</returns>
        public static ITurtlePathBuilder UseCIdFor<TEntity, TTargetType, TDbType>(
            this ITurtlePathBuilder builder,
            Action<CIdConfiguration<TTargetType, TDbType>> configureIdentifier,
            string propertyName = CIdDefinition.DefaultPropertyName)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            AddCIdDefinition(
                builder.Services,
                CreateEntityContext(typeof(TEntity), propertyName),
                typeof(TEntity),
                propertyName,
                configureIdentifier);

            return builder;
        }

        private static void AddCIdDefinition<TTargetType, TDbType>(
            IServiceCollection services,
            string context,
            Type entityType,
            string propertyName,
            Action<CIdConfiguration<TTargetType, TDbType>> setup)
        {
            if (setup == null)
                throw new ArgumentNullException(nameof(setup));

            var config = new CIdConfiguration<TTargetType, TDbType>();
            setup(config);
            config.ValidateAndThrow();

            var registry = GetOrCreateRegistry(services);
            registry.Register(new CIdDefinition(
                context,
                entityType,
                propertyName,
                typeof(TTargetType),
                config.DefaultFactory,
                config.ParseFunction,
                id => id.ToString(),
                id => config.ToByteArrayFunction((TTargetType)id.Value),
                config.GenerationStrategy,
                typeof(TDbType),
                config.DbType,
                config.ConvertToDb,
                config.ConvertFromDb));
        }

        private static CIdDefinitionRegistry GetOrCreateRegistry(IServiceCollection services)
        {
            var registry = services
                .Select(descriptor => descriptor.ImplementationInstance)
                .OfType<CIdDefinitionRegistry>()
                .FirstOrDefault();

            if (registry != null)
                return registry;

            registry = new CIdDefinitionRegistry();
            services.AddSingleton(registry);
            services.AddSingleton<ICIdDefinitionRegistry>(registry);
            services.AddSingleton<ICIdFactory>(registry);

            return registry;
        }

        private static string CreateEntityContext(Type entityType, string propertyName)
            => $"{entityType.FullName}.{propertyName ?? CIdDefinition.DefaultPropertyName}";
    }
}
