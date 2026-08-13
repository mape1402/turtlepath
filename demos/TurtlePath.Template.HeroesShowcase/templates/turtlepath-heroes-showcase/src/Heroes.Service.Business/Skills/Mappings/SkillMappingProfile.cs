using Heroes.Service.Business.Skills.Models.Requests;
using Heroes.Service.Business.Skills.Models.Responses;
using Heroes.Service.Domain;
using OctoMap;

namespace Heroes.Service.Business.Skills.Mappings;

public sealed class SkillMappingProfile : OctoMapProfile
{
    public override void Configure(IOctoMapConfigurationBuilder builder)
    {
        builder.CreateMap<CreateHeroSkillRequest, Skill>();
        builder.CreateMap<CreateVillainSkillRequest, Skill>();
        builder.CreateMap<Skill, SkillResponse>();
    }
}
