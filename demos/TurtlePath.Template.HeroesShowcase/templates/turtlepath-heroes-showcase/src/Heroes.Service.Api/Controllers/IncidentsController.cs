using Heroes.Service.Business.Incidents.Models.Requests;
using Heroes.Service.Business.Incidents.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using TurtlePath.Domain.Identifier;
using TurtlePath.Spider;

namespace Heroes.Service.Api.Controllers;

/// <summary>
/// REST endpoints for incidents.
/// </summary>
[Route("incidents")]
public sealed class IncidentsController : BaseController
{
    /// <summary>
    /// Assigns an incident using a fully custom command handler.
    /// </summary>
    [HttpPost("{id}/assign")]
    public Task<IncidentResponse> Assign([FromRoute] CId id, [FromBody] AssignIncidentRequest request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Spider.DefaultSend<AssignIncidentRequest, IncidentResponse>(request, cancellationToken);
    }

    /// <summary>
    /// Resolves an incident using a custom handler.
    /// </summary>
    [HttpPost("{id}/resolve")]
    public Task<IncidentResponse> Resolve([FromRoute] CId id, [FromBody] ResolveIncidentRequest request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Spider.DefaultSend<ResolveIncidentRequest, IncidentResponse>(request, cancellationToken);
    }
}
