namespace Microsoft.Extensions.DependencyInjection
{
    using global::Crabalidator.DependencyInjection;
    using TurtlePath;
    using TurtlePath.Application.Hooks;
    using TurtlePath.Crabalidator;
    using TurtlePath.EntityFrameworkCore;
    using TurtlePath.Mapping;
    using TurtlePath.Validation;
    using TurtlePath.Persistence;
    using TurtlePath.OctoMap;
    using global::OctoMap;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    /// <summary>
    /// Provides dependency injection extensions for the TurtlePath composition package.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ServicesExtensions
    {
        /// <summary>
        /// Registers the default TurtlePath stack.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddTurtlePath(this IServiceCollection services)
            => services.AddTurtlePath(_ => { });

        /// <summary>
        /// Registers the default TurtlePath stack.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configure">The registration options.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddTurtlePath(this IServiceCollection services, Action<TurtlePathOptions> configure)
        {
            var options = new TurtlePathOptions();
            configure?.Invoke(options);

            services.AddScoped<IStorageReaderAdapter, StorageReaderAdapter>();
            services.AddScoped<IStorageWriterAdapter, StorageWriterAdapter>();
            services.AddScoped<IMapperAdapter, MapperAdapter>();
            services.AddScoped<IValidatorAdapter, ValidatorAdapter>();

            var assembliesToScan = new[] { typeof(AssemblyReference).Assembly }
                .Concat(options.ApplicationAssemblies)
                .Distinct()
                .ToArray();

            services.AddCrabalidator(assembliesToScan);

            services.AddOctoMap(registration =>
            {
                registration.Options.EnableRuntimeImplicitMaps = true;
                registration.Options.DuplicateMapPolicy = DuplicateMapPolicy.Throw;
                foreach (var assembly in assembliesToScan)
                    registration.AddMaps(assembly);
            });

            services.AddTurtlePathSieve();
            services.AddHandlerHooksFromAssemblies(assembliesToScan);

            return services;
        }

        /// <summary>
        /// Registers the default TurtlePath stack.
        /// </summary>
        /// <param name="services">The service collection.</param>
        [Obsolete("Use AddTurtlePath instead.")]
        public static void AddBusiness(this IServiceCollection services)
            => services.AddTurtlePath();

        /// <summary>
        /// Registers the default TurtlePath stack and discovers handler hooks from the specified assemblies.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="hookAssemblies">The assemblies to scan for handler hooks.</param>
        [Obsolete("Use AddTurtlePath(options => options.AddApplicationAssemblies(...)) instead.")]
        public static void AddBusiness(this IServiceCollection services, params Assembly[] hookAssemblies)
            => services.AddTurtlePath(options => options.AddApplicationAssemblies(hookAssemblies));
    }
}
