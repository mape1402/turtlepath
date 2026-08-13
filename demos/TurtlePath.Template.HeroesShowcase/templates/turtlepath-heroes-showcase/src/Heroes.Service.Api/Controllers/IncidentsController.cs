using Heroes.Service.Business.Incidents.Models.Requests;
using Heroes.Service.Business.Incidents.Models.Responses;
using Heroes.Service.Business.Incidents.Queries;
using Microsoft.AspNetCore.Mvc;
using TurtlePath.Spider;
using TurtlePath.Domain.Identifier;
using TurtlePath.Models.Responses;
using TurtlePath.Queries;

namespace Heroes.Service.Api.Controllers;

/// <summary>
/// REST endpoints for incidents.
/// </summary>
[Route("incidents")]
public sealed class IncidentsController : BaseController
{
    /// <summary>
    /// Reports an incident using automation and incident hooks.
    /// </summary>
    [HttpPost]
    public Task<IncidentResponse> Report([FromBody] ReportIncidentRequest request, CancellationToken cancellationToken)
        => Spider.DefaultSend<ReportIncidentRequest, IncidentResponse>(request, cancellationToken);

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

    /// <summary>
    /// Gets one incident by id.
    /// </summary>
    [HttpGet("{id}")]
    public Task<IncidentResponse> GetById([FromRoute] CId id, CancellationToken cancellationToken)
        => Spider.DefaultSend<GetIncidentByIdQuery, IncidentResponse>(new GetIncidentByIdQuery(id), cancellationToken);

    /// <summary>
    /// Gets paged incidents using DataScorpio and a typed status filter.
    /// </summary>
    [HttpGet]
    public Task<PagedResponse<IncidentResponse>> GetPaged([FromQuery] PagedSettings pagedSettings, CancellationToken cancellationToken)
        => Spider.DefaultSend<GetPagedIncidentsQuery, PagedResponse<IncidentResponse>>(new GetPagedIncidentsQuery(pagedSettings), cancellationToken);
}
