using Heroes.Service.Domain.Enums;
using TurtlePath.Domain.Identifier;
using TurtlePath.Models.Responses;

namespace Heroes.Service.Business.Incidents.Models.Responses;

public sealed class IncidentResponse : BaseResponse
{
    public string Title { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public ThreatLevel ThreatLevel { get; set; }

    public IncidentStatus Status { get; set; }

    public CId? AssignedHeroId { get; set; }

    public CId? SuspectedVillainId { get; set; }

    public DateTimeOffset ReportedAt { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }
}
