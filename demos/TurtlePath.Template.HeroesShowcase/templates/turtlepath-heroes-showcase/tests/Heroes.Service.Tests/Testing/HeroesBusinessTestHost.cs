using Heroes.Service.Business.Jobs;
using Heroes.Service.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using TurtlePath.Jobs;

namespace Heroes.Service.Tests.Testing;

/// <summary>
/// Creates a complete test host for the Heroes demo using the same dependency graph as the API host.
/// </summary>
public sealed class HeroesBusinessTestHost : IAsyncDisposable
{
    private readonly string _databasePath;

    private HeroesBusinessTestHost(ServiceProvider serviceProvider, string databasePath)
    {
        Services = serviceProvider;
        _databasePath = databasePath;
    }

    /// <summary>
    /// Gets the service provider created for the current test.
    /// </summary>
    public ServiceProvider Services { get; }

    /// <summary>
    /// Creates a complete host with a disposable SQLite database.
    /// </summary>
    public static async Task<HeroesBusinessTestHost> CreateAsync(bool seedUniverse = true)
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:Default"] = $"Data Source={databasePath}",
                ["Pigeon:Domain"] = "Heroes.Service",
                ["Pigeon:MessageBrokers:AzureServiceBus:ConnectionString"] = "",
                ["Pigeon:Outbox:Enabled"] = "false",
                ["TransactionBoundary:Enabled"] = "true",
                ["TransactionBoundary:IncludeQueries"] = "false",
                ["TransactionBoundary:IsolationLevel"] = "ReadCommitted",
                ["TransactionBoundary:TimeoutSeconds"] = "30"
            })
            .Build();

        services.AddLogging();
        services.AddDefaults(configuration, new TestWebHostEnvironment());

        var provider = services.BuildServiceProvider();
        var host = new HeroesBusinessTestHost(provider, databasePath);

        await host.CreateSchemaAsync();

        if (seedUniverse)
            await host.ExecuteJobAsync<SeedHeroesUniverseJob>();

        return host;
    }

    /// <summary>
    /// Creates a scoped service provider for handler or database work.
    /// </summary>
    public AsyncServiceScope CreateScope() => Services.CreateAsyncScope();

    /// <summary>
    /// Runs a registered one-shot or cron job inside a fresh scope.
    /// </summary>
    public async Task ExecuteJobAsync<TJob>()
        where TJob : TurtlePathJob
    {
        await using var scope = CreateScope();

        await scope.ServiceProvider
            .GetRequiredService<TJob>()
            .ExecuteAsync(new TurtlePathJobContext(), CancellationToken.None);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await Services.DisposeAsync();

        try
        {
            File.Delete(_databasePath);
        }
        catch (IOException)
        {
        }
    }

    private async Task CreateSchemaAsync()
    {
        await using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Heroes.Service.Tests";

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public string EnvironmentName { get; set; } = Environments.Development;

        public string WebRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
