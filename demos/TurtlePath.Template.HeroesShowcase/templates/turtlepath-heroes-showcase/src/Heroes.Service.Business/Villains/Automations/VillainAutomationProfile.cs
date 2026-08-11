using Heroes.Service.Business.Villains.Models.Requests;
using Heroes.Service.Business.Villains.Models.Responses;
using Heroes.Service.Business.Villains.Queries;
using Heroes.Service.Domain;
using TurtlePath.Automations.Profiles;

namespace Heroes.Service.Business.Villains.Automations;

/// <summary>
/// Declares generated handlers for villain CRUD and capture workflows.
/// </summary>
public sealed class VillainAutomationProfile : TurtlePathAutomationProfile
{
    /// <inheritdoc />
    public override void Configure(ITurtlePathAutomationBuilder builder)
    {
        builder.For<Villain>()
            .ToCreate<CreateVillainRequest, VillainResponse>(options => options.Include(villain => villain.Team))
            .ToUpdate<UpdateVillainRequest, VillainResponse>(options => options.Include(villain => villain.Team))
            .ToPatch<CaptureVillainRequest, VillainResponse>(options => options.Include(villain => villain.Team));
    }
}
