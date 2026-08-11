using Heroes.Service.Business.Villains.Models.Requests;
using Heroes.Service.Business.Villains.Models.Responses;
using Heroes.Service.Business.Villains.Queries;
using Microsoft.AspNetCore.Mvc;
using TurtlePath.Spider;
using TurtlePath.Domain.Identifier;
using TurtlePath.Models.Responses;
using TurtlePath.Queries;

namespace Heroes.Service.Api.Controllers;

/// <summary>
/// REST endpoints for villains.
/// </summary>
[Route("villains")]
public sealed class VillainsController : BaseController
{
    /// <summary>
    /// Creates a villain using automation.
    /// </summary>
    [HttpPost]
    public Task<VillainResponse> Create([FromBody] CreateVillainRequest request, CancellationToken cancellationToken)
        => Spider.DefaultSend<CreateVillainRequest, VillainResponse>(request, cancellationToken);

    /// <summary>
    /// Updates a villain using automation.
    /// </summary>
    [HttpPut("{id}")]
    public Task<VillainResponse> Update([FromRoute] CId id, [FromBody] UpdateVillainRequest request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Spider.DefaultSend<UpdateVillainRequest, VillainResponse>(request, cancellationToken);
    }

    /// <summary>
    /// Captures a villain through patch automation.
    /// </summary>
    [HttpPost("{id}/capture")]
    public Task<VillainResponse> Capture([FromRoute] CId id, CancellationToken cancellationToken)
        => Spider.DefaultSend<CaptureVillainRequest, VillainResponse>(new CaptureVillainRequest { Id = id }, cancellationToken);

    /// <summary>
    /// Gets a villain by id.
    /// </summary>
    [HttpGet("{id}")]
    public Task<VillainResponse> GetById([FromRoute] CId id, CancellationToken cancellationToken)
        => Spider.DefaultSend<GetVillainByIdQuery, VillainResponse>(new GetVillainByIdQuery(id), cancellationToken);

    /// <summary>
    /// Gets paged villains with DataScorpio filters and sorts.
    /// </summary>
    [HttpGet]
    public Task<PagedResponse<VillainResponse>> GetPaged([FromQuery] PagedSettings pagedSettings, CancellationToken cancellationToken)
        => Spider.DefaultSend<GetPagedVillainsQuery, PagedResponse<VillainResponse>>(new GetPagedVillainsQuery(pagedSettings), cancellationToken);
}
