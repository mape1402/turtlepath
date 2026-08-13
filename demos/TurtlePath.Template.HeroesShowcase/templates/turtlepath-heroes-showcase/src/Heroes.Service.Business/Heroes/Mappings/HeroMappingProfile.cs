using Heroes.Service.Business.Heroes.Models.Requests;
using Heroes.Service.Business.Heroes.Models.Responses;
using Heroes.Service.Domain;
using OctoMap;

namespace Heroes.Service.Business.Heroes.Mappings;

public sealed class HeroMappingProfile : OctoMapProfile
{
    public override void Configure(IOctoMapConfigurationBuilder builder)
    {
        builder.CreateMap<CreateHeroRequest, Hero>();
        builder.CreateMap<UpdateHeroRequest, Hero>();
        builder.CreateMap<Hero, HeroResponse>()
            .ForMember(response => response.TeamName, options => options.MapFrom(hero => hero.Team == null ? string.Empty : hero.Team.Name));
    }
}
