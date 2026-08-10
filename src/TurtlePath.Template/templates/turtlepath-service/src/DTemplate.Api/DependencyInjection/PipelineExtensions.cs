using DTemplate.Api.Boundaries;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    internal static class PipelineExtensions
    {
        internal static IServiceCollection AddPipelineDefaults(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransactionBoundaryDefaults(configuration);

            services.AddSpider(builder =>
            {
                builder.AddExecutionBoundary<TransactionExecutionBoundary>();
            });

            return services;
        }
    }
}
