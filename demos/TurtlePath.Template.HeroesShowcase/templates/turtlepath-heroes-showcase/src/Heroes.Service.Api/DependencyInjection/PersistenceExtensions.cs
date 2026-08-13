using System.Diagnostics.CodeAnalysis;
using Heroes.Service.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides dependency injection extensions for persistence.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class PersistenceExtensions
    {
        /// <summary>
        /// Registers persistence services using the configured connection string.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddPersistenceDefaults(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default");

            services.AddDbContext<AppDbContext>(opts =>
            {
                // SQLite keeps the showcase runnable from a clean checkout without infrastructure.
                opts.UseSqlite(
                    connectionString,
                    sqlite => sqlite.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            });

            return services;
        }
    }
}
