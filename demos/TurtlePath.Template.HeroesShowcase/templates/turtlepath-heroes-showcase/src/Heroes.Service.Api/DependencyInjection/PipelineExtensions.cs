using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    internal static class PipelineExtensions
    {
        internal static IServiceCollection AddPipelineDefaults(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTurtlePathSpiderTransactions(configuration);

            return services;
        }
    }
}
