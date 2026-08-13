using Heroes.Service.Domain.Enums;
using TurtlePath.Models.Responses;

namespace Heroes.Service.Business.Villains.Models.Responses;

public sealed class VillainResponse : BaseResponse
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
    /// Gets or sets a value indicating whether the villain has been captured.
    /// </summary>
    public bool Captured { get; set; }

    /// <summary>
    /// Gets or sets the display name of the related team.
    /// </summary>
    public string TeamName { get; set; } = string.Empty;
}
