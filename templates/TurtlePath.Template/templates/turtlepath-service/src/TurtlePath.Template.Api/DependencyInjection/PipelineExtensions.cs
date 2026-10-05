using System.Diagnostics.CodeAnalysis;
using TurtlePath.Template.Business;

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

            services.AddSpiderDevelopmentDefaults(environment);

            return services;
        }
    }
}
