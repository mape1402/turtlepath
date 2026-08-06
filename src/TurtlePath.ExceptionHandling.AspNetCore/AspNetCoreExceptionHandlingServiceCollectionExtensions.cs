using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
    }
}
