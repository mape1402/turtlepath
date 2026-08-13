using Heroes.Service.Business.Heroes.Models.Requests;
using Heroes.Service.Business.Heroes.Models.Responses;
using Heroes.Service.Business.Heroes.Queries;
using Microsoft.AspNetCore.Mvc;
using TurtlePath.Spider;
using TurtlePath.Domain.Identifier;
using TurtlePath.Models.Responses;
using TurtlePath.Queries;

namespace Heroes.Service.Api.Controllers;

/// <summary>
/// REST endpoints for heroes.
/// </summary>
[Route("heroes")]
public sealed class HeroesController : BaseController
{
    /// <summary>
    /// Creates a hero using TurtlePath automations.
    /// </summary>
    [HttpPost]
    public Task<HeroResponse> Create([FromBody] CreateHeroRequest request, CancellationToken cancellationToken)
        => Spider.DefaultSend<CreateHeroRequest, HeroResponse>(request, cancellationToken);

    /// <summary>
    /// Updates a hero using TurtlePath automations.
    /// </summary>
    [HttpPut("{id}")]
    public Task<HeroResponse> Update([FromRoute] CId id, [FromBody] UpdateHeroRequest request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Spider.DefaultSend<UpdateHeroRequest, HeroResponse>(request, cancellationToken);
    }

    /// <summary>
    /// Deactivates a hero through a no-response patch automation.
    /// </summary>
    [HttpPost("{id}/deactivate")]
    public Task Deactivate([FromRoute] CId id, CancellationToken cancellationToken)
        => Spider.DefaultSend(new DeactivateHeroRequest { Id = id }, cancellationToken);

    /// <summary>
    /// Gets an operations report using a custom ADO.NET read model service behind a query handler.
    /// </summary>
    [HttpGet("operations-report")]
    public Task<HeroOperationsReportResponse> GetOperationsReport([FromQuery] CId? teamId, CancellationToken cancellationToken)
        => Spider.DefaultSend<GetHeroOperationsReportQuery, HeroOperationsReportResponse>(new GetHeroOperationsReportQuery { TeamId = teamId }, cancellationToken);

    /// <summary>
    /// Gets one hero by id.
    /// </summary>
    [HttpGet("{id}")]
    public Task<HeroResponse> GetById([FromRoute] CId id, CancellationToken cancellationToken)
        => Spider.DefaultSend<GetHeroByIdQuery, HeroResponse>(new GetHeroByIdQuery(id), cancellationToken);

    /// <summary>
    /// Gets paged heroes with DataScorpio filters, sorts and search.
    /// </summary>
    [HttpGet]
    public Task<PagedResponse<HeroResponse>> GetPaged([FromQuery] PagedSettings pagedSettings, [FromQuery] CId? teamId, CancellationToken cancellationToken)
        => Spider.DefaultSend<GetPagedHeroesQuery, PagedResponse<HeroResponse>>(new GetPagedHeroesQuery(pagedSettings) { TeamId = teamId }, cancellationToken);
}
