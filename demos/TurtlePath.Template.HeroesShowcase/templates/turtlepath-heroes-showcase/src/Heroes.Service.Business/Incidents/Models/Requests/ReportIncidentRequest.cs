using Heroes.Service.Business.Incidents.Models.Responses;
using Heroes.Service.Domain.Enums;
using Pelican.Mediator;
using TurtlePath.Domain.Identifier;

namespace Heroes.Service.Business.Incidents.Models.Requests;

public sealed record ReportIncidentRequest(
    string Title,
    string City,
    ThreatLevel ThreatLevel,
    CId? SuspectedVillainId) : IRequest<IncidentResponse>;
