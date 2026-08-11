using Heroes.Service.Business.Teams.Models.Requests;
using Heroes.Service.Business.Teams.Models.Responses;
using Heroes.Service.Domain;
using OctoMap;

namespace Heroes.Service.Business.Teams.Mappings;

public sealed class TeamMappingProfile : OctoMapProfile
{
    public override void Configure(IOctoMapConfigurationBuilder builder)
    {
        builder.CreateMap<CreateTeamRequest, Team>();
        builder.CreateMap<UpdateTeamRequest, Team>();
        builder.CreateMap<Team, TeamResponse>();
    }
}
