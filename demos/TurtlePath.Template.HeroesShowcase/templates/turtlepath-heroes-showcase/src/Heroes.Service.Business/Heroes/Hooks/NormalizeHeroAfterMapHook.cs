using Heroes.Service.Business.Heroes.Models.Requests;
using Heroes.Service.Domain;
using TurtlePath.Hooks;

namespace Heroes.Service.Business.Heroes.Hooks;

/// <summary>
/// Demonstrates a feature-specific hook that cleans mapped entity data without replacing the handler.
/// </summary>
public sealed class NormalizeHeroAfterMapHook :
    IAfterMapHook<CreateHeroRequest, Hero>,
    IAfterMapHook<UpdateHeroRequest, Hero>
{
    /// <inheritdoc />
    public ValueTask AfterMapAsync(CommandHookContext<CreateHeroRequest, Hero> context, CancellationToken cancellationToken = default)
    {
        Normalize(context.Entity);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask AfterMapAsync(CommandHookContext<UpdateHeroRequest, Hero> context, CancellationToken cancellationToken = default)
    {
        Normalize(context.Entity);
        return ValueTask.CompletedTask;
    }

    private static void Normalize(Hero hero)
    {
        hero.Alias = hero.Alias.Trim();
        hero.RealName = hero.RealName.Trim();
        hero.City = hero.City.Trim();
    }
}
