using Heroes.Service.Domain.Enums;
using TurtlePath.Domain.Contracts;
using TurtlePath.Domain.Identifier;

namespace Heroes.Service.Domain;

/// <summary>
/// Represents an ability owned by either a hero or a villain.
/// </summary>
public sealed class Skill : BaseEntity
{
    /// <summary>
    /// Gets or sets the skill name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owner alignment so the same entity can support two creation flows.
    /// </summary>
    public Alignment OwnerAlignment { get; set; }

    /// <summary>
    /// Gets or sets the skill mastery score.
    /// </summary>
    public int Mastery { get; set; }

    /// <summary>
    /// Gets or sets the hero owner when this is a hero skill.
    /// </summary>
    public CId? HeroId { get; set; }

    /// <summary>
    /// Gets or sets the hero navigation property.
    /// </summary>
    public Hero Hero { get; set; }

    /// <summary>
    /// Gets or sets the villain owner when this is a villain skill.
    /// </summary>
    public CId? VillainId { get; set; }

    /// <summary>
    /// Gets or sets the villain navigation property.
    /// </summary>
    public Villain Villain { get; set; }
}
