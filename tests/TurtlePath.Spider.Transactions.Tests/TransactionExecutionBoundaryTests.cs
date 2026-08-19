using TurtlePath.Spider.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Spider.Pipelines.Boundaries;
using System.Transactions;

namespace TurtlePath.Template.Tests;

public sealed class TransactionExecutionBoundaryTests
{
    [Fact]
    public void Registration_does_not_discover_profiles_from_test_assemblies()
    {
        var services = new ServiceCollection();

        services.AddTurtlePathSpiderTransactions(
            new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build(),
            typeof(TransactionExecutionBoundary).Assembly);

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IOptions<TransactionBoundaryOptions>>().Value);
    }

    [Fact]
    public async Task BeginAsync_opens_transaction_for_commands()
    {
        var options = Options.Create(new TransactionBoundaryOptions());
        var boundary = new TransactionExecutionBoundary(options, new TransactionBoundaryRequestFilter(options));
        var context = CreateContext(typeof(SampleCommand));

        try
        {
            await boundary.BeginAsync(context, CancellationToken.None);

            Assert.NotNull(Transaction.Current);

            await boundary.CompleteAsync(context, CancellationToken.None);

            Assert.Null(Transaction.Current);
        }
        finally
        {
            if (Transaction.Current != null)
                await boundary.CancelAsync(context, CancellationToken.None);
        }
    }

    [Fact]
    public async Task BeginAsync_skips_queries_by_default()
    {
        var options = Options.Create(new TransactionBoundaryOptions());
        var boundary = new TransactionExecutionBoundary(options, new TransactionBoundaryRequestFilter(options));
        var context = CreateContext(typeof(SampleQuery));

        await boundary.BeginAsync(context, CancellationToken.None);

        Assert.Null(Transaction.Current);
    }

    [Fact]
    public async Task BeginAsync_skips_requests_marked_with_attribute()
    {
        var options = Options.Create(new TransactionBoundaryOptions());
        var boundary = new TransactionExecutionBoundary(options, new TransactionBoundaryRequestFilter(options));
        var context = CreateContext(typeof(SampleSkippedCommand));

        await boundary.BeginAsync(context, CancellationToken.None);

        Assert.Null(Transaction.Current);
    }

    [Fact]
    public void RequestFilter_discovers_and_caches_boundary_decisions()
    {
        var options = Options.Create(new TransactionBoundaryOptions
        {
            ExcludedRequestTypes = new HashSet<string> { nameof(SampleExcludedCommand) }
        });

        var filter = new TransactionBoundaryRequestFilter(options);

        filter.Discover(typeof(TransactionExecutionBoundaryTests).Assembly);

        Assert.True(filter.ShouldOpenTransaction(typeof(SampleCommand)));
        Assert.False(filter.ShouldOpenTransaction(typeof(SampleQuery)));
        Assert.False(filter.ShouldOpenTransaction(typeof(SampleSkippedCommand)));
        Assert.False(filter.ShouldOpenTransaction(typeof(SampleExcludedCommand)));
    }

    [Fact]
    public void RequestFilter_uses_options_configured_by_code()
    {
        var boundaryOptions = new TransactionBoundaryOptions()
            .DiscoverRequestsFrom<SampleCommand>()
            .Exclude<SampleExcludedCommand>();

        var filter = new TransactionBoundaryRequestFilter(Options.Create(boundaryOptions));

        filter.Discover(boundaryOptions.RequestAssemblies.ToArray());

        Assert.True(filter.ShouldOpenTransaction(typeof(SampleCommand)));
        Assert.False(filter.ShouldOpenTransaction(typeof(SampleExcludedCommand)));
    }

    private static PipelineExecutionContext CreateContext(Type requestType)
        => new()
        {
            RequestType = requestType,
            Request = Activator.CreateInstance(requestType),
            Services = new ServiceCollection().BuildServiceProvider()
        };

    private sealed class SampleCommand
    {
    }

    private sealed class SampleQuery
    {
    }

    [SkipTransactionBoundary]
    private sealed class SampleSkippedCommand
    {
    }

    private sealed class SampleExcludedCommand
    {
    }

    private sealed class ThrowingTestProfile : ITransactionBoundaryProfile
    {
        public void Configure(TransactionBoundaryOptions options)
        {
            throw new InvalidOperationException("Test profiles must not be discovered by application registration.");
        }
    }
}
