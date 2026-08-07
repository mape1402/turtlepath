namespace TurtlePath.Testing.EntityFrameworkCore
{
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using TurtlePath.EntityFrameworkCore;

    /// <summary>
    /// Provides Entity Framework Core testing helpers for TurtlePath test hosts.
    /// </summary>
    public static class TurtlePathEntityFrameworkCoreTestingExtensions
    {
        /// <summary>
        /// Registers an in-memory SQLite database and uses TurtlePath EF Core storage adapters.
        /// </summary>
        public static TurtlePathTestHostBuilder UseSqliteDbContext<TDbContext>(
            this TurtlePathTestHostBuilder builder,
            Func<TurtlePathDbContextOptions, TurtlePathDbContextOptions> configureTurtlePath = null,
            Action<IServiceProvider, DbContextOptionsBuilder> configureDbContext = null)
            where TDbContext : DbContext, IDbContext
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.WithoutInMemoryStorage();
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(provider =>
                {
                    var connection = new SqliteConnection("Data Source=:memory:");
                    connection.Open();

                    return connection;
                });

                services.AddDbContext<TDbContext>((provider, options) =>
                {
                    options.UseSqlite(provider.GetRequiredService<SqliteConnection>());
                    configureDbContext?.Invoke(provider, options);
                });

                services
                    .AddTurtlePath()
                    .UseEntityFrameworkCore<TDbContext>(configureTurtlePath);
            });

            return builder;
        }

        /// <summary>
        /// Creates the SQLite schema for the configured DbContext.
        /// </summary>
        public static async Task CreateSchemaAsync<TDbContext>(
            this TurtlePathTestHost host,
            CancellationToken cancellationToken = default)
            where TDbContext : DbContext
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            var dbContext = host.Resolve<TDbContext>();
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        /// <summary>
        /// Drops and recreates the SQLite schema for the configured DbContext.
        /// </summary>
        public static async Task ResetDatabaseAsync<TDbContext>(
            this TurtlePathTestHost host,
            CancellationToken cancellationToken = default)
            where TDbContext : DbContext
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            var dbContext = host.Resolve<TDbContext>();
            await dbContext.Database.EnsureDeletedAsync(cancellationToken);
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        /// <summary>
        /// Adds seed entities through the DbContext and saves changes.
        /// </summary>
        public static async Task SeedAsync<TDbContext, TEntity>(
            this TurtlePathTestHost host,
            params TEntity[] entities)
            where TDbContext : DbContext
            where TEntity : class
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            var dbContext = host.Resolve<TDbContext>();
            await dbContext.Set<TEntity>().AddRangeAsync(entities);
            await dbContext.SaveChangesAsync();
        }
    }
}
