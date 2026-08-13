using TurtlePath.Models.Responses;

namespace Heroes.Service.Business.Heroes.Models.Responses;

public sealed class HeroResponse : BaseResponse
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
    /// Gets or sets the city associated with the resource.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the relative power level used by demo business rules.
    /// </summary>
    public int PowerLevel { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the hero is active for assignments.
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets the display name of the related team.
    /// </summary>
    public string TeamName { get; set; } = string.Empty;
}
