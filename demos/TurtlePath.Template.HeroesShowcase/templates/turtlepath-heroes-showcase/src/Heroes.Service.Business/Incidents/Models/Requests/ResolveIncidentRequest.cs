using Heroes.Service.Business.Incidents.Models.Responses;
using Pelican.Mediator;
using TurtlePath.Models.Requests;

namespace Heroes.Service.Business.Incidents.Models.Requests;

public sealed class ResolveIncidentRequest : BaseRequest, IRequest<IncidentResponse>
{
    public string ResolutionNotes { get; set; } = string.Empty;
}
