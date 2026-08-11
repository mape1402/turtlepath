using Heroes.Service.Domain.Contracts;
using TurtlePath.Domain.Contracts;
using TurtlePath.Domain.Identifier;

namespace Heroes.Service.Domain;

/// <summary>
/// Represents a hero managed through the recommended TurtlePath happy path.
/// </summary>
public sealed class Hero : BaseEntity, ITeamMember
{
    /// <summary>
    /// Gets or sets the public hero name used by the API.
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the civilian identity for richer mapping examples.
    /// </summary>
    public string RealName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the city where the hero normally operates.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the numeric power score used by filters, jobs and handlers.
    /// </summary>
    public int PowerLevel { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the hero can receive assignments.
    /// </summary>
    public bool Active { get; set; } = true;

    /// <inheritdoc />
    public CId TeamId { get; set; }

    /// <summary>
    /// Gets or sets the team navigation property.
    /// </summary>
    public Team Team { get; set; }

    /// <summary>
    /// Gets or sets the skill catalog owned by the hero.
    /// </summary>
    public ICollection<Skill> Skills { get; set; } = [];
}
