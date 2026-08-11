using Heroes.Service.Business.Teams.Models.Requests;
using Heroes.Service.Business.Teams.Queries;
using Heroes.Service.Business.Teams.Validators;
using Heroes.Service.Domain;
using Heroes.Service.Persistence;
using Heroes.Service.Tests.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pelican.Mediator;

namespace Heroes.Service.Tests;

/// <summary>
/// Demonstrates unit and integration tests for the Team feature.
/// </summary>
public sealed class TeamsEntityTests
{
    /// <summary>
    /// Shows a fast unit test for a Crabalidator validator.
    /// </summary>
    [Fact]
    public void Create_team_validator_rejects_incomplete_requests()
    {
        var validator = new CreateTeamRequestValidator();

        var result = validator.Validate(new CreateTeamRequest("", "", ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateTeamRequest.Name));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateTeamRequest.City));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateTeamRequest.Headquarters));
    }

    /// <summary>
    /// Shows a full integration test for create, update, get-by-id and get-many query handlers.
    /// </summary>
    [Fact]
    public async Task Team_happy_path_supports_create_update_get_by_id_and_filtered_list()
    {
        await using var host = await HeroesBusinessTestHost.CreateAsync();
        await using var scope = host.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<AppDbContext>();
        var mediator = services.GetRequiredService<IMediator>();

        var created = await mediator.Send(new CreateTeamRequest("Sentinel Guard", "Central City", "Pulse Tower"));
        var updated = await mediator.Send(new UpdateTeamRequest
        {
            Id = created.Id,
            Name = "Sentinel Guard Prime",
            City = "Central City",
            Headquarters = "Pulse Tower North"
        });
        var byId = await mediator.Send(new GetTeamByIdQuery(created.Id));
        var centralCityTeams = await mediator.Send(new GetTeamsQuery { City = "central city" });
        var savedTeam = await dbContext.Set<Team>().SingleAsync(team => team.Id == created.Id);

        Assert.Equal("Sentinel Guard Prime", updated.Name);
        Assert.Equal(updated.Name, byId.Name);
        Assert.Equal("Pulse Tower North", savedTeam.Headquarters);
        Assert.Contains(centralCityTeams, team => team.Id == created.Id);
    }
}
