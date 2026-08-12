using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using TurtlePath.ExceptionHandling;

namespace TurtlePath.ExceptionHandling.Workers
{
    /// <summary>
    /// Provides service registration extensions for background exception handling.
    /// </summary>
    public static class WorkerExceptionHandlingServiceCollectionExtensions
    {
        /// <summary>
        /// Registers background exception handling adapters.
        /// </summary>
        public static IServiceCollection AddTurtlePathWorkerExceptionHandling(
            this IServiceCollection services,
            Action<BackgroundExceptionHandlingOptionsBuilder> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddTurtlePathExceptionHandlingCore();

            services.Configure<BackgroundExceptionHandlingOptions>(options =>
            {
                var builder = new BackgroundExceptionHandlingOptionsBuilder(options);
                configure?.Invoke(builder);
            });

            services.TryAddSingleton<IBackgroundExceptionReporter, LoggingBackgroundExceptionReporter>();
            services.TryAddSingleton<IBackgroundExceptionBoundary, BackgroundExceptionBoundary>();

            return services;
        }

        /// <summary>
        /// Registers a background exception handling profile.
        /// </summary>
        public static IServiceCollection AddBackgroundExceptionHandlingProfile<TProfile>(this IServiceCollection services)
            where TProfile : IBackgroundExceptionHandlingProfile, new()
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddBackgroundExceptionHandlingProfile(new TProfile());
        }

        /// <summary>
        /// Registers a background exception handling profile.
        /// </summary>
        public static IServiceCollection AddBackgroundExceptionHandlingProfile(
            this IServiceCollection services,
            IBackgroundExceptionHandlingProfile profile)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            return services.AddTurtlePathWorkerExceptionHandling(profile.Configure);
        }

        /// <summary>
        /// Discovers and registers background exception handling profiles from the supplied assemblies.
        /// </summary>
        public static IServiceCollection AddBackgroundExceptionHandlingProfiles(
            this IServiceCollection services,
            params Assembly[] profileAssemblies)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (profileAssemblies == null || profileAssemblies.Length == 0)
                return services;

            foreach (var profile in CreateProfiles(profileAssemblies))
                services.AddBackgroundExceptionHandlingProfile(profile);

            return services;
        }

        private static IEnumerable<IBackgroundExceptionHandlingProfile> CreateProfiles(IEnumerable<Assembly> profileAssemblies)
        {
            return profileAssemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    typeof(IBackgroundExceptionHandlingProfile).IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    type.GetConstructor(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        Type.EmptyTypes,
                        null) != null)
                .Select(type => (IBackgroundExceptionHandlingProfile)Activator.CreateInstance(type, nonPublic: true));
        }
    }
}
