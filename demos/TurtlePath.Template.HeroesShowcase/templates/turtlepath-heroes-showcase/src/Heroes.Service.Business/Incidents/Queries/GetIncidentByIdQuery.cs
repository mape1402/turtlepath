using Heroes.Service.Business.Incidents.Models.Responses;
using Heroes.Service.Domain;
using TurtlePath.Domain.Identifier;
using TurtlePath.Queries;

namespace Heroes.Service.Business.Incidents.Queries;

public sealed class GetIncidentByIdQuery : GetByIdQuery<Incident, IncidentResponse>
{
    public GetIncidentByIdQuery(CId id) : base(id)
    {
    }
}
