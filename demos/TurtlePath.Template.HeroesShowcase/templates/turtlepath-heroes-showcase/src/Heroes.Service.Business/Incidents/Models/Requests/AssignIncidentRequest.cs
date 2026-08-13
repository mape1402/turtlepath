using Heroes.Service.Business.Incidents.Models.Responses;
using Pelican.Mediator;
using TurtlePath.Domain.Identifier;
using TurtlePath.Models.Requests;

namespace Heroes.Service.Business.Incidents.Models.Requests;

public sealed class AssignIncidentRequest : BaseRequest, IRequest<IncidentResponse>
{
    /// <summary>
    /// Gets or sets the hero identifier used by the request.
    /// </summary>
    public CId HeroId { get; set; }
}
