using Heroes.Service.Business.Skills.Models.Requests;
using Heroes.Service.Business.Skills.Models.Responses;
using Heroes.Service.Domain;
using TurtlePath.Automations.Profiles;

namespace Heroes.Service.Business.Skills.Automations;

/// <summary>
/// Demonstrates repeated create automations for the same entity with different request models.
/// </summary>
public sealed class SkillAutomationProfile : TurtlePathAutomationProfile
{
    /// <inheritdoc />
    public override void Configure(ITurtlePathAutomationBuilder builder)
    {
        builder.For<Skill>()
            .ToCreate<CreateHeroSkillRequest, SkillResponse>()
            .ToCreate<CreateVillainSkillRequest, SkillResponse>();
    }
}
