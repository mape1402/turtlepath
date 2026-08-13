using Heroes.Service.Business.Villains.Models.Requests;
using Heroes.Service.Business.Villains.Models.Responses;
using Heroes.Service.Domain;
using OctoMap;

namespace Heroes.Service.Business.Villains.Mappings;

public sealed class VillainMappingProfile : OctoMapProfile
{
    public override void Configure(IOctoMapConfigurationBuilder builder)
    {
        builder.CreateMap<CreateVillainRequest, Villain>();
        builder.CreateMap<UpdateVillainRequest, Villain>();
        builder.CreateMap<Villain, VillainResponse>()
            .ForMember(response => response.TeamName, options => options.MapFrom(villain => villain.Team == null ? string.Empty : villain.Team.Name));
    }
}
