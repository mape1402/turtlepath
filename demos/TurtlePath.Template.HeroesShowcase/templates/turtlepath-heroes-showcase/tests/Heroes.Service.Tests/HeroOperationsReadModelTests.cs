using Heroes.Service.Business.Heroes.Queries;
using Heroes.Service.Business.Jobs;
using Heroes.Service.Tests.Testing;
using Microsoft.Extensions.DependencyInjection;
using Pelican.Mediator;

namespace Heroes.Service.Tests;

/// <summary>
/// Demonstrates tests for specialized read models that intentionally bypass the default EF query handlers.
/// </summary>
public sealed class HeroOperationsReadModelTests
{
    /// <summary>
    /// Verifies that a custom query handler can delegate to an ADO.NET service while the rest of the app still uses TurtlePath defaults.
    /// </summary>
    [Fact]
    public async Task Operations_report_uses_the_custom_read_model_service()
    {
        await using var host = await HeroesBusinessTestHost.CreateAsync();

        await host.ExecuteJobAsync<AutoAssignOpenIncidentsJob>();

        await using var scope = host.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var report = await mediator.Send(new GetHeroOperationsReportQuery(), CancellationToken.None);

        Assert.Equal(2, report.ActiveHeroes);
        Assert.Equal(1, report.OpenAssignments);
        Assert.Contains(report.Heroes, row => row.Alias == "Solar Sentinel" && row.AssignedOpenIncidents == 1);
        Assert.Contains(report.Heroes, row => row.Alias == "Night Wolf" && row.AssignedOpenIncidents == 0);
    }
}
