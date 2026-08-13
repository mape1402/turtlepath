using Heroes.Service.Domain.Enums;
using TurtlePath.Domain.Contracts;
using TurtlePath.Domain.Identifier;

namespace Heroes.Service.Domain;

/// <summary>
/// Represents an operational incident that can be reported, assigned and resolved.
/// </summary>
public sealed class Incident : BaseEntity
{
    /// <summary>
    /// Gets or sets the short incident title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the city where the incident is happening.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the incident threat level used by assignment rules.
    /// </summary>
    public ThreatLevel ThreatLevel { get; set; } = ThreatLevel.Low;

    /// <summary>
    /// Gets or sets the current incident workflow status.
    /// </summary>
    public IncidentStatus Status { get; set; } = IncidentStatus.Reported;

    /// <summary>
    /// Gets or sets the hero assigned to resolve the incident.
    /// </summary>
    public CId? AssignedHeroId { get; set; }

    /// <summary>
    /// Gets or sets the assigned hero navigation property.
    /// </summary>
    public Hero AssignedHero { get; set; }

    /// <summary>
    /// Gets or sets the suspected villain when intelligence has a match.
    /// </summary>
    public CId? SuspectedVillainId { get; set; }

    /// <summary>
    /// Gets or sets the suspected villain navigation property.
    /// </summary>
    public Villain SuspectedVillain { get; set; }

    /// <summary>
    /// Gets or sets the date when the incident was reported.
    /// </summary>
    public DateTimeOffset ReportedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the date when the incident was resolved.
    /// </summary>
    public DateTimeOffset? ResolvedAt { get; set; }
}
