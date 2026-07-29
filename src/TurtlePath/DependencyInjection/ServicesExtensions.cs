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
    /// Provides dependency injection extensions for the business layer.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ServicesExtensions
    {
        /// <summary>
        /// Registers business-layer services.
        /// </summary>
        /// <param name="services">The service collection.</param>
        public static void AddBusiness(this IServiceCollection services)
            => services.AddBusiness(Array.Empty<Assembly>());

        /// <summary>
        /// Registers business-layer services and discovers handler hooks from the specified assemblies.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="hookAssemblies">The assemblies to scan for handler hooks.</param>
        public static void AddBusiness(this IServiceCollection services, params Assembly[] hookAssemblies)
        {
            services.AddScoped<IStorageReaderAdapter, StorageReaderAdapter>();
            services.AddScoped<IStorageWriterAdapter, StorageWriterAdapter>();
            services.AddScoped<IMapperAdapter, MapperAdapter>();
            services.AddScoped<IValidatorAdapter, ValidatorAdapter>();

            services.AddCrabalidator(typeof(AssemblyReference).Assembly);

            services.AddOctoMap(registration =>
            {
                registration.Options.EnableRuntimeImplicitMaps = true;
                registration.Options.DuplicateMapPolicy = DuplicateMapPolicy.Throw;
                registration.AddMaps(typeof(AssemblyReference).Assembly);
            });

            services.AddTurtlePathSieve();

            var assembliesToScan = new[] { typeof(AssemblyReference).Assembly }
                .Concat(hookAssemblies ?? Array.Empty<Assembly>())
                .ToArray();

            services.AddHandlerHooksFromAssemblies(assembliesToScan);
        }
    }
}
