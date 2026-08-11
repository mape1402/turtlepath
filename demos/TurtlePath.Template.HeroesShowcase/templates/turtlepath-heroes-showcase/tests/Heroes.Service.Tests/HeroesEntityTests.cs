using Heroes.Service.Business.Heroes.Models.Requests;
using Heroes.Service.Business.Heroes.Models.Responses;
using Heroes.Service.Business.Heroes.Queries;
using Heroes.Service.Business.Heroes.Validators;
using Heroes.Service.Business.Services.Audit;
using Heroes.Service.Domain;
using Heroes.Service.Persistence;
using Heroes.Service.Tests.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pelican.Mediator;
using TurtlePath.Domain.Identifier;
using TurtlePath.Models.Responses;
using TurtlePath.Queries;

namespace Heroes.Service.Tests;

/// <summary>
/// Demonstrates unit and integration tests for the Hero feature.
/// </summary>
public sealed class HeroesEntityTests
{
    /// <summary>
    /// Shows a unit test for request validation without database dependencies.
    /// </summary>
    [Fact]
    public void Create_hero_validator_rejects_invalid_power_and_missing_team()
    {
        var validator = new CreateHeroRequestValidator();

        var result = validator.Validate(new CreateHeroRequest("A", "B", "C", 101, CId.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateHeroRequest.PowerLevel));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateHeroRequest.TeamId));
    }

    /// <summary>
    /// Shows an automation integration test with mapping hooks and audit hooks.
    /// </summary>
    [Fact]
    public async Task Hero_automation_trims_input_persists_entity_and_records_audit()
    {
        await using var host = await HeroesBusinessTestHost.CreateAsync();
        await using var scope = host.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<AppDbContext>();
        var mediator = services.GetRequiredService<IMediator>();
        var auditTrail = services.GetRequiredService<IAuditTrail>();
        var team = await dbContext.Set<Team>().SingleAsync(item => item.Name == "Justice League");

        var response = await mediator.Send(
            new CreateHeroRequest("  Vector Saint  ", "  Iris Nolan  ", " Star City ", 83, team.Id),
            CancellationToken.None);
        var savedHero = await dbContext.Set<Hero>().SingleAsync(item => item.Id == response.Id);
        var byId = await mediator.Send(new GetHeroByIdQuery(response.Id), CancellationToken.None);

        Assert.Equal("Vector Saint", response.Alias);
        Assert.Equal("Vector Saint", byId.Alias);
        Assert.Equal("Iris Nolan", savedHero.RealName);
        Assert.Equal("Star City", savedHero.City);
        Assert.Equal(team.Id, savedHero.TeamId);
        Assert.Contains(auditTrail.Entries, entry => entry.Contains("Hero created: Vector Saint.", StringComparison.Ordinal));
    }

    /// <summary>
    /// Shows update and no-response patch command coverage over generated automation handlers.
    /// </summary>
    [Fact]
    public async Task Hero_update_and_deactivate_commands_modify_the_same_entity()
    {
        await using var host = await HeroesBusinessTestHost.CreateAsync();
        await using var scope = host.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<AppDbContext>();
        var mediator = services.GetRequiredService<IMediator>();
        var hero = await dbContext.Set<Hero>().SingleAsync(item => item.Alias == "Night Wolf");

        var updated = await mediator.Send(new UpdateHeroRequest
        {
            Id = hero.Id,
            Alias = "Night Wolf Prime",
            RealName = "Marcus Vale",
            City = "Gotham",
            PowerLevel = 79,
            TeamId = hero.TeamId
        });
        await mediator.Send(new DeactivateHeroRequest { Id = hero.Id });
        var savedHero = await dbContext.Set<Hero>().SingleAsync(item => item.Id == hero.Id);

        Assert.Equal("Night Wolf Prime", updated.Alias);
        Assert.Equal(79, savedHero.PowerLevel);
        Assert.False(savedHero.Active);
    }

    /// <summary>
    /// Shows a paged query handler override combined with DataScorpio filters and sorts.
    /// </summary>
    [Fact]
    public async Task Paged_hero_query_applies_handler_filters_and_datascorpio_filters()
    {
        await using var host = await HeroesBusinessTestHost.CreateAsync();
        await using var scope = host.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<AppDbContext>();
        var mediator = services.GetRequiredService<IMediator>();
        var team = await dbContext.Set<Team>().SingleAsync(item => item.Name == "Justice League");

        dbContext.Set<Hero>().Add(new Hero
        {
            Id = CId.From(Ulid.NewUlid()),
            Alias = "Retired Comet",
            RealName = "Nora Hale",
            City = "Metropolis",
            PowerLevel = 99,
            TeamId = team.Id,
            Active = false
        });
        await dbContext.SaveChangesAsync();

        var page = await mediator.Send(
            new GetPagedHeroesQuery(new PagedSettings
            {
                Filters = "Elite==true",
                Sorts = "-power",
                PageNumber = 1,
                PageSize = 10
            })
            {
                TeamId = team.Id
            },
            CancellationToken.None);

        Assert.IsType<PagedResponse<HeroResponse>>(page);
        Assert.Equal(["Solar Sentinel"], page.Results.Select(hero => hero.Alias));
        Assert.All(page.Results, hero => Assert.True(hero.Active));
        Assert.All(page.Results, hero => Assert.Equal("Justice League", hero.TeamName));
    }
}
