#if (JobHost)
using TurtlePath.Template.Api.Boundaries;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Pelican.Mediator;
using Spider.Pipelines.Core;
using TurtlePath.Domain.Identifier;
using TurtlePath.EntityFrameworkCore;
using TurtlePath.ExceptionHandling;
using TurtlePath.Jobs;
using TurtlePath.Persistence;

namespace TurtlePath.Template.Tests;

public sealed class JobCompositionTests
{
    [Fact]
    public void AddJobDefaults_registers_job_infrastructure()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();
        var environment = new TestHostEnvironment();

        services.AddLogging();
        services.AddJobDefaults(configuration, environment);

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true
        });

        using var scope = provider.CreateScope();
        var scopedProvider = scope.ServiceProvider;

        Assert.NotNull(scopedProvider.GetRequiredService<IMediator>());
        Assert.NotNull(scopedProvider.GetRequiredService<ISpider>());
        Assert.NotNull(scopedProvider.GetRequiredService<ICIdFactory>());
        Assert.NotNull(scopedProvider.GetRequiredService<IDbContext>());
        Assert.NotNull(scopedProvider.GetRequiredService<IStorageReaderAdapter>());
        Assert.NotNull(scopedProvider.GetRequiredService<IStorageWriterAdapter>());
        Assert.NotNull(scopedProvider.GetRequiredService<IExceptionHandler>());
        Assert.NotNull(scopedProvider.GetRequiredService<ITurtlePathJobManager>());

        var transactionOptions = provider.GetRequiredService<IOptions<TransactionBoundaryOptions>>().Value;

        Assert.True(transactionOptions.Enabled);
        Assert.False(transactionOptions.IncludeQueries);
        Assert.Equal(30, transactionOptions.TimeoutSeconds);
    }

    private static IConfiguration CreateConfiguration()
    {
        var values = new Dictionary<string, string>
        {
            ["ConnectionStrings:Default"] = "Server=(localdb)\\mssqllocaldb;Database=TurtlePath.Template_JobComposition;Trusted_Connection=True;MultipleActiveResultSets=true",
            ["TransactionBoundary:Enabled"] = "true",
            ["TransactionBoundary:IncludeQueries"] = "false",
            ["TransactionBoundary:IsolationLevel"] = "ReadCommitted",
            ["TransactionBoundary:TimeoutSeconds"] = "30"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string ApplicationName { get; set; } = "TurtlePath.Template.Tests";

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public string EnvironmentName { get; set; } = Environments.Development;
    }
}
#endif
