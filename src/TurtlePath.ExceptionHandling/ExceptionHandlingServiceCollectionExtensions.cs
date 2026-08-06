using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace TurtlePath.ExceptionHandling
{
    /// <summary>
    /// Provides service registration extensions for TurtlePath exception handling core.
    /// </summary>
    public static class ExceptionHandlingServiceCollectionExtensions
    {
        /// <summary>
        /// Registers TurtlePath transport-neutral exception handling services.
        /// </summary>
        public static IServiceCollection AddTurtlePathExceptionHandlingCore(
            this IServiceCollection services,
            Action<ExceptionHandlingOptionsBuilder> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.Configure<ExceptionHandlingOptions>(options =>
            {
                var builder = new ExceptionHandlingOptionsBuilder(options);
                configure?.Invoke(builder);
            });

            services.TryAddSingleton<IExceptionHandler, DefaultExceptionHandler>();

            return services;
        }

        /// <summary>
        /// Registers a transport-neutral exception handling profile.
        /// </summary>
        public static IServiceCollection AddExceptionHandlingProfile<TProfile>(this IServiceCollection services)
            where TProfile : IExceptionHandlingProfile, new()
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddExceptionHandlingProfile(new TProfile());
        }

        /// <summary>
        /// Registers a transport-neutral exception handling profile.
        /// </summary>
        public static IServiceCollection AddExceptionHandlingProfile(
            this IServiceCollection services,
            IExceptionHandlingProfile profile)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            return services.AddTurtlePathExceptionHandlingCore(profile.Configure);
        }

        /// <summary>
        /// Discovers and registers transport-neutral exception handling profiles from the supplied assemblies.
        /// </summary>
        public static IServiceCollection AddExceptionHandlingProfiles(
            this IServiceCollection services,
            params Assembly[] profileAssemblies)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (profileAssemblies == null || profileAssemblies.Length == 0)
                return services;

            foreach (var profile in CreateProfiles(profileAssemblies))
                services.AddExceptionHandlingProfile(profile);

            return services;
        }

        private static IEnumerable<IExceptionHandlingProfile> CreateProfiles(IEnumerable<Assembly> profileAssemblies)
        {
            return profileAssemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    typeof(IExceptionHandlingProfile).IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    type.GetConstructor(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        Type.EmptyTypes,
                        null) != null)
                .Select(type => (IExceptionHandlingProfile)Activator.CreateInstance(type, nonPublic: true));
        }
    }
}
