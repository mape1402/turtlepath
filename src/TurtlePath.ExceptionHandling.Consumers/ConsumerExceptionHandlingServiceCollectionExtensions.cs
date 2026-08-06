using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
    }
}
