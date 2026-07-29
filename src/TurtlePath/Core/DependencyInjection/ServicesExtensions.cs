namespace Microsoft.Extensions.DependencyInjection
{
    using Crabalidator.DependencyInjection;
    using TurtlePath;
    using TurtlePath.Core.Hooks;
    using TurtlePath.Core.Infrastructure;
    using TurtlePath.Core.Services;
    using TurtlePath.MappingProfiles;
    using OctoMap;
    using Sieve.Services;
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

            services.AddCrabalidator(typeof(Constants).Assembly);

            services.AddOctoMap(registration =>
            {
                registration.Options.EnableRuntimeImplicitMaps = true;
                registration.Options.DuplicateMapPolicy = DuplicateMapPolicy.Throw;
                registration.AddProfile<MappingProfile>();
                registration.AddMaps(typeof(Constants).Assembly);
            });

            services.AddSingleton<ISieveProcessor, SieveProcessor>();

            var assembliesToScan = new[] { typeof(Constants).Assembly }
                .Concat(hookAssemblies ?? Array.Empty<Assembly>())
                .ToArray();

            services.AddHandlerHooksFromAssemblies(assembliesToScan);
        }
    }
}
