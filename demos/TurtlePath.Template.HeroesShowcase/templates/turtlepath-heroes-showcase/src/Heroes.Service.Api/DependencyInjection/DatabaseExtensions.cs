using Heroes.Service.Business.Jobs;
using Heroes.Service.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using TurtlePath.Jobs;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides database startup helpers for the SQLite showcase.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class DatabaseExtensions
    {
        /// <summary>
        /// Creates the SQLite schema when it does not exist so the demo can run from a clean folder.
        /// </summary>
        /// <param name="app">The web application.</param>
        /// <returns>A task that completes when the database schema has been checked.</returns>
        internal static async Task<WebApplication> UseDatabaseDefaultsAsync(this WebApplication app)
        {
            await using var scope = app.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // The showcase does not ship migrations. EnsureCreated keeps first-run setup frictionless.
            await dbContext.Database.EnsureCreatedAsync();

            if (app.Configuration.GetValue("Demo:SeedOnStartup", false))
            {
                var seedJob = scope.ServiceProvider.GetRequiredService<SeedHeroesUniverseJob>();
                await seedJob.ExecuteAsync(new TurtlePathJobContext(), app.Lifetime.ApplicationStopping);
            }

            return app;
        }
    }
}
