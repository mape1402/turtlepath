using Heroes.Service.Business.Villains.Models.Responses;
using Heroes.Service.Domain.Enums;
using Pelican.Mediator;
using TurtlePath.Domain.Identifier;
using TurtlePath.Models.Requests;

namespace Heroes.Service.Business.Villains.Models.Requests;

public sealed class UpdateVillainRequest : BaseRequest, IRequest<VillainResponse>
{
    /// <summary>
    /// Gets or sets the public codename used by the character.
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character civilian identity.
    /// </summary>
    public string RealName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the villain lair.
    /// </summary>
    public string Lair { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the relative power level used by demo business rules.
    /// </summary>
    public int PowerLevel { get; set; }

    /// <summary>
    /// Gets or sets the threat level assigned to the resource.
    /// </summary>
    public ThreatLevel ThreatLevel { get; set; }

    /// <summary>
    /// Gets or sets the team identifier associated with the resource.
    /// </summary>
    public CId TeamId { get; set; }
}
