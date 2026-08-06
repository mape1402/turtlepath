using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
    }
}
