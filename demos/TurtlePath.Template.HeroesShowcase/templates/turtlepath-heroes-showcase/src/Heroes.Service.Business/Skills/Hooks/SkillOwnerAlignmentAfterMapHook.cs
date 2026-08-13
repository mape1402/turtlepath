using Heroes.Service.Business.Skills.Models.Requests;
using Heroes.Service.Domain;
using Heroes.Service.Domain.Enums;
using TurtlePath.Hooks;

namespace Heroes.Service.Business.Skills.Hooks;

/// <summary>
/// Completes skill ownership metadata after request-to-entity mapping.
/// </summary>
public sealed class SkillOwnerAlignmentAfterMapHook :
    IAfterMapHook<CreateHeroSkillRequest, Skill>,
    IAfterMapHook<CreateVillainSkillRequest, Skill>
{
    /// <summary>
    /// Marks skills created from the hero endpoint as hero-owned.
    /// </summary>
    public ValueTask AfterMapAsync(CommandHookContext<CreateHeroSkillRequest, Skill> context, CancellationToken cancellationToken = default)
    {
        context.Entity.OwnerAlignment = Alignment.Hero;
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Marks skills created from the villain endpoint as villain-owned.
    /// </summary>
    public ValueTask AfterMapAsync(CommandHookContext<CreateVillainSkillRequest, Skill> context, CancellationToken cancellationToken = default)
    {
        context.Entity.OwnerAlignment = Alignment.Villain;
        return ValueTask.CompletedTask;
    }
}
