using Heroes.Service.Business.Incidents.Models.Requests;
using Heroes.Service.Business.Incidents.Models.Responses;
using Heroes.Service.Business.Incidents.Queries;
using Heroes.Service.Business.Incidents.Validators;
using Heroes.Service.Business.Services.Audit;
using Heroes.Service.Domain;
using Heroes.Service.Domain.Enums;
using Heroes.Service.Persistence;
using Heroes.Service.Tests.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pelican.Mediator;
using Spider.Pipelines.Core;
using TurtlePath.Models.Responses;
using TurtlePath.Spider;
using TurtlePath.Queries;

namespace Heroes.Service.Tests;

/// <summary>
/// Demonstrates unit and integration tests for the Incident feature.
/// </summary>
public sealed class IncidentsEntityTests
{
    /// <summary>
    /// Shows a unit test for incident validation rules.
    /// </summary>
    [Fact]
    public void Resolve_incident_validator_rejects_missing_notes()
    {
        var validator = new ResolveIncidentRequestValidator();

        var result = validator.Validate(new ResolveIncidentRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ResolveIncidentRequest.Id));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ResolveIncidentRequest.ResolutionNotes));
    }

    /// <summary>
    /// Shows generated report automation with a before-save hook and a get-by-id automation.
    /// </summary>
    [Fact]
    public async Task Incident_report_automation_sets_defaults_and_can_be_read_by_id()
    {
        await using var host = await HeroesBusinessTestHost.CreateAsync();
        await using var scope = host.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<AppDbContext>();
        var mediator = services.GetRequiredService<IMediator>();
        var auditTrail = services.GetRequiredService<IAuditTrail>();
        var villain = await dbContext.Set<Villain>().SingleAsync(item => item.Alias == "Cipher Queen");

        var reported = await mediator.Send(new ReportIncidentRequest("Museum breach", "Gotham", ThreatLevel.Medium, villain.Id));
        var byId = await mediator.Send(new GetIncidentByIdQuery(reported.Id));

        Assert.Equal(IncidentStatus.Reported, reported.Status);
        Assert.Equal(reported.Title, byId.Title);
        Assert.NotEqual(default, byId.ReportedAt);
        Assert.Contains(auditTrail.Entries, entry => entry.Contains("Incident reported: Museum breach.", StringComparison.Ordinal));
    }

    /// <summary>
    /// Shows custom command handlers through the Spider to Pelican bridge used by controllers.
    /// </summary>
    [Fact]
    public async Task Incident_custom_handlers_assign_and_resolve_incidents_through_spider()
    {
        await using var host = await HeroesBusinessTestHost.CreateAsync();
        await using var scope = host.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<AppDbContext>();
        var spider = services.GetRequiredService<ISpider>();
        var auditTrail = services.GetRequiredService<IAuditTrail>();
        var incident = await dbContext.Set<Incident>().SingleAsync(item => item.Title == "City grid blackout");
        var hero = await dbContext.Set<Hero>().SingleAsync(item => item.Alias == "Solar Sentinel");

        var assigned = await spider.DefaultSend<AssignIncidentRequest, IncidentResponse>(
            new AssignIncidentRequest { Id = incident.Id, HeroId = hero.Id },
            CancellationToken.None);
        var resolved = await spider.DefaultSend<ResolveIncidentRequest, IncidentResponse>(
            new ResolveIncidentRequest { Id = incident.Id, ResolutionNotes = "Grid restored." },
            CancellationToken.None);

        Assert.Equal(IncidentStatus.Assigned, assigned.Status);
        Assert.Equal(hero.Id, assigned.AssignedHeroId);
        Assert.Equal(IncidentStatus.Resolved, resolved.Status);
        Assert.NotNull(resolved.ResolvedAt);
        Assert.Contains(auditTrail.Entries, entry => entry.Contains("assigned to hero", StringComparison.Ordinal));
        Assert.Contains(auditTrail.Entries, entry => entry.Contains("resolved. Notes: Grid restored.", StringComparison.Ordinal));
    }

    /// <summary>
    /// Shows generated paged query handlers for incidents.
    /// </summary>
    [Fact]
    public async Task Incident_paged_query_supports_datascorpio_sorting()
    {
        await using var host = await HeroesBusinessTestHost.CreateAsync();
        await using var scope = host.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var page = await mediator.Send(
            new GetPagedIncidentsQuery(new PagedSettings
            {
                Sorts = "-threat",
                PageNumber = 1,
                PageSize = 10
            }));

        Assert.IsType<PagedResponse<IncidentResponse>>(page);
        Assert.Contains(page.Results, incident => incident.Title == "City grid blackout");
    }
}
