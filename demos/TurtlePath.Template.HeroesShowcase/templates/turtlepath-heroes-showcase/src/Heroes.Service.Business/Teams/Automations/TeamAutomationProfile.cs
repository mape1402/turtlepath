using Heroes.Service.Business.Teams.Models.Requests;
using Heroes.Service.Business.Teams.Models.Responses;
using Heroes.Service.Business.Teams.Queries;
using Heroes.Service.Domain;
using TurtlePath.Automations.Profiles;

namespace Heroes.Service.Business.Teams.Automations;

/// <summary>
/// Declares generated handlers for teams while leaving list queries as a custom handler example.
/// </summary>
public sealed class TeamAutomationProfile : TurtlePathAutomationProfile
{
    /// <inheritdoc />
    public override void Configure(ITurtlePathAutomationBuilder builder)
    {
        builder.For<Team>()
            .ToCreate<CreateTeamRequest, TeamResponse>()
            .ToUpdate<UpdateTeamRequest, TeamResponse>()
            .ToGetById<GetTeamByIdQuery, TeamResponse>();
    }
}
