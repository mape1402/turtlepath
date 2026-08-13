using Heroes.Service.Domain.Enums;
using TurtlePath.Domain.Identifier;
using TurtlePath.Models.Responses;

namespace Heroes.Service.Business.Incidents.Models.Responses;

public sealed class IncidentResponse : BaseResponse
{
    /// <summary>
    /// Gets or sets the incident title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the city associated with the resource.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the threat level assigned to the resource.
    /// </summary>
    public ThreatLevel ThreatLevel { get; set; }

    /// <summary>
    /// Gets or sets the current incident status.
    /// </summary>
    public IncidentStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the hero assigned to the incident.
    /// </summary>
    public CId? AssignedHeroId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the suspected villain.
    /// </summary>
    public CId? SuspectedVillainId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the incident was reported.
    /// </summary>
    public DateTimeOffset ReportedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the incident was resolved.
    /// </summary>
    public DateTimeOffset? ResolvedAt { get; set; }
}
