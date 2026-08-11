using Heroes.Service.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// Provides dependency injection extensions for persistence.
    /// </summary>
    public static class PersistenceExtensions
    {
        /// <summary>
        /// Registers persistence services using the provided connection string.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="connectionString">The database connection string.</param>
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
