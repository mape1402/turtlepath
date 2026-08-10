using DTemplate.Api.Boundaries;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    internal static class TransactionBoundaryExtensions
    {
        internal static IServiceCollection AddTransactionBoundaryDefaults(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TransactionBoundaryOptions>(configuration.GetSection("TransactionBoundary"));
            services.AddSingleton<ITransactionBoundaryRequestFilter>(provider =>
            {
                var filter = new TransactionBoundaryRequestFilter(provider.GetRequiredService<IOptions<TransactionBoundaryOptions>>());
                filter.Discover(typeof(DTemplate.Business.Constants).Assembly);

                return filter;
            });

            return services;
        }
    }
}
