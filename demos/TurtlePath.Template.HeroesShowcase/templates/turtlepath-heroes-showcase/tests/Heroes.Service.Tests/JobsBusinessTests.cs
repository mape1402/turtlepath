using Heroes.Service.Business.Jobs;
using Heroes.Service.Business.Services.Audit;
using Heroes.Service.Domain;
using Heroes.Service.Domain.Enums;
using Heroes.Service.Persistence;
using Heroes.Service.Tests.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Heroes.Service.Tests;

/// <summary>
/// Demonstrates tests for one-shot and recurring job implementations.
/// </summary>
public sealed class JobsBusinessTests
{
    /// <summary>
    /// Shows a one-shot job integration test against a disposable SQLite database.
    /// </summary>
    [Fact]
    public async Task Seed_job_creates_the_demo_universe_once()
    {
        await using var host = await HeroesBusinessTestHost.CreateAsync(seedUniverse: false);

        await host.ExecuteJobAsync<SeedHeroesUniverseJob>();
        await host.ExecuteJobAsync<SeedHeroesUniverseJob>();

        await using var scope = host.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<AppDbContext>();
        var auditTrail = services.GetRequiredService<IAuditTrail>();

        Assert.Equal(2, await dbContext.Set<Team>().CountAsync());
        Assert.Equal(2, await dbContext.Set<Hero>().CountAsync());
        Assert.Single(await dbContext.Set<Villain>().ToListAsync());
        Assert.Contains(auditTrail.Entries, entry => entry.Contains("Seeded heroes universe demo data.", StringComparison.Ordinal));
    }

    /// <summary>
    /// Shows cron job tests that execute services directly rather than adding unnecessary handlers.
    /// </summary>
    [Fact]
    public async Task Cron_jobs_assign_open_incidents_and_recalculate_team_reputation()
    {
        await using var host = await HeroesBusinessTestHost.CreateAsync();

        await host.ExecuteJobAsync<AutoAssignOpenIncidentsJob>();
        await host.ExecuteJobAsync<RecalculateTeamReputationJob>();

        await using var scope = host.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<AppDbContext>();
        var auditTrail = services.GetRequiredService<IAuditTrail>();
        var incident = await dbContext.Set<Incident>().SingleAsync(item => item.Title == "City grid blackout");
        var heroTeam = await dbContext.Set<Team>().SingleAsync(item => item.Name == "Justice League");
        var villainTeam = await dbContext.Set<Team>().SingleAsync(item => item.Name == "Rogues Gallery");

        Assert.Equal(IncidentStatus.Assigned, incident.Status);
        Assert.NotNull(incident.AssignedHeroId);
        Assert.Equal(100, heroTeam.Reputation);
        Assert.Equal(-88, villainTeam.Reputation);
        Assert.Contains(auditTrail.Entries, entry => entry.Contains("Auto-assigned incident", StringComparison.Ordinal));
        Assert.Contains(auditTrail.Entries, entry => entry.Contains("Recalculated team reputation.", StringComparison.Ordinal));
    }
}
