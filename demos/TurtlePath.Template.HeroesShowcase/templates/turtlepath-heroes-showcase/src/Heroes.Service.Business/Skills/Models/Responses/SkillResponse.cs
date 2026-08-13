using Heroes.Service.Domain.Enums;
using TurtlePath.Domain.Identifier;
using TurtlePath.Models.Responses;

namespace Heroes.Service.Business.Skills.Models.Responses;

public sealed class SkillResponse : BaseResponse
{
    /// <summary>
    /// Gets or sets the display name of the resource.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owner alignment for the skill.
    /// </summary>
    public Alignment OwnerAlignment { get; set; }

    /// <summary>
    /// Gets or sets the skill mastery level.
    /// </summary>
    public int Mastery { get; set; }

    /// <summary>
    /// Gets or sets the hero identifier used by the request.
    /// </summary>
    public CId? HeroId { get; set; }

    /// <summary>
    /// Gets or sets the villain identifier associated with the resource.
    /// </summary>
    public CId? VillainId { get; set; }
}
