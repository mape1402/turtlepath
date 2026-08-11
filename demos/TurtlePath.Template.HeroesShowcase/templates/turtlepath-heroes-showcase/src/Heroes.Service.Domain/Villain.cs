using Heroes.Service.Domain.Contracts;
using Heroes.Service.Domain.Enums;
using TurtlePath.Domain.Contracts;
using TurtlePath.Domain.Identifier;

namespace Heroes.Service.Domain;

/// <summary>
/// Represents an antagonist that can be queried, captured and linked to incidents.
/// </summary>
public sealed class Villain : BaseEntity, ITeamMember
{
    /// <summary>
    /// Gets or sets the villain alias shown to clients.
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the known real name when intelligence has it.
    /// </summary>
    public string RealName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the villain operating base.
    /// </summary>
    public string Lair { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the score used for threat sorting and assignment decisions.
    /// </summary>
    public int PowerLevel { get; set; }

    /// <summary>
    /// Gets or sets the coarse business severity for the villain.
    /// </summary>
    public ThreatLevel ThreatLevel { get; set; } = ThreatLevel.Medium;

    /// <summary>
    /// Gets or sets a value indicating whether the villain has already been captured.
    /// </summary>
    public bool Captured { get; set; }

    /// <inheritdoc />
    public CId TeamId { get; set; }

    /// <summary>
    /// Gets or sets the team navigation property.
    /// </summary>
    public Team Team { get; set; }

    /// <summary>
    /// Gets or sets the villain skill catalog.
    /// </summary>
    public ICollection<Skill> Skills { get; set; } = [];
}
