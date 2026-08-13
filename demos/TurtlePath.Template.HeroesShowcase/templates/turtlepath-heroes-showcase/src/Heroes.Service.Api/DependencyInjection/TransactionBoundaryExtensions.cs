using Heroes.Service.Api.Boundaries.Transactions;
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
                filter.Discover(typeof(Heroes.Service.Business.Constants).Assembly);

                return filter;
            });

            return services;
        }
    }
}
