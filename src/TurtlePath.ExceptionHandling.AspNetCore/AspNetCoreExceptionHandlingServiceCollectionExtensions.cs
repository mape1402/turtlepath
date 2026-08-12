using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace TurtlePath.ExceptionHandling.AspNetCore
{
    /// <summary>
    /// Provides service registration extensions for ASP.NET Core exception handling.
    /// </summary>
    public static class AspNetCoreExceptionHandlingServiceCollectionExtensions
    {
        /// <summary>
        /// Registers ASP.NET Core exception handling adapters.
        /// </summary>
        public static IServiceCollection AddTurtlePathAspNetCoreExceptionHandling(
            this IServiceCollection services,
            Action<HttpExceptionHandlingOptionsBuilder> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.Configure<HttpExceptionHandlingOptions>(options =>
            {
                var builder = new HttpExceptionHandlingOptionsBuilder(options);
                configure?.Invoke(builder);
            });

            services.TryAddSingleton<IHttpExceptionStatusCodeMapper, DefaultHttpExceptionStatusCodeMapper>();
            services.TryAddSingleton<IHttpExceptionResponseFactory, ProblemDetailsExceptionResponseFactory>();

            return services;
        }

        /// <summary>
        /// Registers an ASP.NET Core exception handling profile.
        /// </summary>
        public static IServiceCollection AddHttpExceptionHandlingProfile<TProfile>(this IServiceCollection services)
            where TProfile : IHttpExceptionHandlingProfile, new()
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddHttpExceptionHandlingProfile(new TProfile());
        }

        /// <summary>
        /// Registers an ASP.NET Core exception handling profile.
        /// </summary>
        public static IServiceCollection AddHttpExceptionHandlingProfile(
            this IServiceCollection services,
            IHttpExceptionHandlingProfile profile)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            return services.AddTurtlePathAspNetCoreExceptionHandling(profile.Configure);
        }

        /// <summary>
        /// Discovers and registers ASP.NET Core exception handling profiles from the supplied assemblies.
        /// </summary>
        public static IServiceCollection AddHttpExceptionHandlingProfiles(
            this IServiceCollection services,
            params Assembly[] profileAssemblies)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (profileAssemblies == null || profileAssemblies.Length == 0)
                return services;

            foreach (var profile in CreateProfiles(profileAssemblies))
                services.AddHttpExceptionHandlingProfile(profile);

            return services;
        }

        private static IEnumerable<IHttpExceptionHandlingProfile> CreateProfiles(IEnumerable<Assembly> profileAssemblies)
        {
            return profileAssemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    typeof(IHttpExceptionHandlingProfile).IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    type.GetConstructor(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        Type.EmptyTypes,
                        null) != null)
                .Select(type => (IHttpExceptionHandlingProfile)Activator.CreateInstance(type, nonPublic: true));
        }
    }
}
