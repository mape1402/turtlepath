using Heroes.Service.Business.Services.Audit;
using Heroes.Service.Business.Villains.Models.Requests;
using Heroes.Service.Business.Villains.Models.Responses;
using Heroes.Service.Business.Villains.Queries;
using Heroes.Service.Business.Villains.Validators;
using Heroes.Service.Domain;
using Heroes.Service.Domain.Enums;
using Heroes.Service.Persistence;
using Heroes.Service.Tests.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pelican.Mediator;
using TurtlePath.Models.Responses;
using TurtlePath.Queries;

namespace Heroes.Service.Tests;

/// <summary>
/// Demonstrates unit and integration tests for the Villain feature.
/// </summary>
public sealed class VillainsEntityTests
{
    /// <summary>
    /// Shows a unit test for a villain validator.
    /// </summary>
    [Fact]
    public void Create_villain_validator_rejects_invalid_power()
    {
        var validator = new CreateVillainRequestValidator();

        var result = validator.Validate(new CreateVillainRequest("Cipher", "Ada", "Grid", 0, ThreatLevel.High, default));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateVillainRequest.PowerLevel));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateVillainRequest.TeamId));
    }

    /// <summary>
    /// Shows generated create and update automation handlers plus a custom get-by-id handler.
    /// </summary>
    [Fact]
    public async Task Villain_happy_path_supports_create_update_and_get_by_id()
    {
        await using var host = await HeroesBusinessTestHost.CreateAsync();
        await using var scope = host.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<AppDbContext>();
        var mediator = services.GetRequiredService<IMediator>();
        var auditTrail = services.GetRequiredService<IAuditTrail>();
        var team = await dbContext.Set<Team>().SingleAsync(item => item.Name == "Rogues Gallery");

        var created = await mediator.Send(new CreateVillainRequest("Gravity Crown", "Selene Vale", "Orbital Vault", 86, ThreatLevel.Critical, team.Id));
        var updated = await mediator.Send(new UpdateVillainRequest
        {
            Id = created.Id,
            Alias = "Gravity Crown Prime",
            RealName = "Selene Vale",
            Lair = "Orbital Vault",
            PowerLevel = 91,
            ThreatLevel = ThreatLevel.Critical,
            TeamId = team.Id
        });
        var byId = await mediator.Send(new GetVillainByIdQuery(created.Id));

        Assert.Equal("Gravity Crown Prime", updated.Alias);
        Assert.Equal(updated.Alias, byId.Alias);
        Assert.Equal("Rogues Gallery", byId.TeamName);
        Assert.Contains(auditTrail.Entries, entry => entry.Contains("Villain created: Gravity Crown.", StringComparison.Ordinal));
    }

    /// <summary>
    /// Shows a generated patch automation and paged query using DataScorpio sorts.
    /// </summary>
    [Fact]
    public async Task Villain_capture_patch_and_paged_query_use_generated_handlers()
    {
        await using var host = await HeroesBusinessTestHost.CreateAsync();
        await using var scope = host.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<AppDbContext>();
        var mediator = services.GetRequiredService<IMediator>();
        var villain = await dbContext.Set<Villain>().SingleAsync(item => item.Alias == "Cipher Queen");

        var captured = await mediator.Send(new CaptureVillainRequest { Id = villain.Id });
        var page = await mediator.Send(
            new GetPagedVillainsQuery(new PagedSettings
            {
                Sorts = "-power",
                PageNumber = 1,
                PageSize = 10
            }));

        Assert.True(captured.Captured);
        Assert.IsType<PagedResponse<VillainResponse>>(page);
        Assert.Contains(page.Results, item => item.Id == villain.Id && item.TeamName == "Rogues Gallery");
    }
}
