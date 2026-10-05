using System.Diagnostics.CodeAnalysis;
using Heroes.Service.Business;

namespace Microsoft.Extensions.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    internal static class PipelineExtensions
    {
        internal static IServiceCollection AddPipelineDefaults(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            services.AddTurtlePathSpiderTransactions(
                configuration,
                typeof(Constants).Assembly,
                typeof(PipelineExtensions).Assembly);

            services.AddSpider(spider =>
            {
                spider.AddFlowProfile("Heroes.Report", options =>
                {
                    options.TelemetryEnabled = true;
                    options.MetricsEnabled = true;
                });
            });

            services.AddSpiderDevelopmentDefaults(environment);

            return services;
        }
    }
}
