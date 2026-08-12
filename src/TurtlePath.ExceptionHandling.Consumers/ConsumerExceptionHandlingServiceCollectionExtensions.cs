using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using TurtlePath.ExceptionHandling;

namespace TurtlePath.ExceptionHandling.Consumers
{
    /// <summary>
    /// Provides service registration extensions for message consumer exception handling.
    /// </summary>
    public static class ConsumerExceptionHandlingServiceCollectionExtensions
    {
        /// <summary>
        /// Registers message consumer exception handling adapters.
        /// </summary>
        public static IServiceCollection AddTurtlePathConsumerExceptionHandling(
            this IServiceCollection services,
            Action<ConsumerExceptionHandlingOptionsBuilder> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddTurtlePathExceptionHandlingCore();

            services.Configure<ConsumerExceptionHandlingOptions>(options =>
            {
                var builder = new ConsumerExceptionHandlingOptionsBuilder(options);
                configure?.Invoke(builder);
            });

            services.TryAddSingleton<IConsumerExceptionReporter, LoggingConsumerExceptionReporter>();
            services.TryAddSingleton<IConsumerExceptionBoundary, ConsumerExceptionBoundary>();

            return services;
        }

        /// <summary>
        /// Registers a message consumer exception handling profile.
        /// </summary>
        public static IServiceCollection AddConsumerExceptionHandlingProfile<TProfile>(this IServiceCollection services)
            where TProfile : IConsumerExceptionHandlingProfile, new()
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddConsumerExceptionHandlingProfile(new TProfile());
        }

        /// <summary>
        /// Registers a message consumer exception handling profile.
        /// </summary>
        public static IServiceCollection AddConsumerExceptionHandlingProfile(
            this IServiceCollection services,
            IConsumerExceptionHandlingProfile profile)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            return services.AddTurtlePathConsumerExceptionHandling(profile.Configure);
        }

        /// <summary>
        /// Discovers and registers message consumer exception handling profiles from the supplied assemblies.
        /// </summary>
        public static IServiceCollection AddConsumerExceptionHandlingProfiles(
            this IServiceCollection services,
            params Assembly[] profileAssemblies)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (profileAssemblies == null || profileAssemblies.Length == 0)
                return services;

            foreach (var profile in CreateProfiles(profileAssemblies))
                services.AddConsumerExceptionHandlingProfile(profile);

            return services;
        }

        private static IEnumerable<IConsumerExceptionHandlingProfile> CreateProfiles(IEnumerable<Assembly> profileAssemblies)
        {
            return profileAssemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    typeof(IConsumerExceptionHandlingProfile).IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    type.GetConstructor(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        Type.EmptyTypes,
                        null) != null)
                .Select(type => (IConsumerExceptionHandlingProfile)Activator.CreateInstance(type, nonPublic: true));
        }
    }
}
