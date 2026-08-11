using Heroes.Service.Business.Incidents.Models.Responses;
using Heroes.Service.Domain;
using Heroes.Service.Domain.Enums;
using TurtlePath.Queries;

namespace Heroes.Service.Business.Incidents.Queries;

public sealed class GetPagedIncidentsQuery : GetPagedInfoQuery<Incident, IncidentResponse>
{
    public GetPagedIncidentsQuery(PagedSettings pagedSettings) : base(pagedSettings ?? new PagedSettings())
    {
    }

    public IncidentStatus? Status { get; set; }
}
