using Heroes.Service.Business.Teams.Models.Requests;
using Heroes.Service.Business.Teams.Models.Responses;
using Heroes.Service.Business.Teams.Queries;
using Microsoft.AspNetCore.Mvc;
using TurtlePath.Spider;
using TurtlePath.Domain.Identifier;

namespace Heroes.Service.Api.Controllers;

/// <summary>
/// REST endpoints for teams and squads.
/// </summary>
[Route("teams")]
public sealed class TeamsController : BaseController
{
    /// <summary>
    /// Creates a team.
    /// </summary>
    [HttpPost]
    public Task<TeamResponse> Create([FromBody] CreateTeamRequest request, CancellationToken cancellationToken)
        => Spider.DefaultSend<CreateTeamRequest, TeamResponse>(request, cancellationToken);

    /// <summary>
    /// Updates a team.
    /// </summary>
    [HttpPut("{id}")]
    public Task<TeamResponse> Update([FromRoute] CId id, [FromBody] UpdateTeamRequest request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Spider.DefaultSend<UpdateTeamRequest, TeamResponse>(request, cancellationToken);
    }

    /// <summary>
    /// Gets a team by id.
    /// </summary>
    [HttpGet("{id}")]
    public Task<TeamResponse> GetById([FromRoute] CId id, CancellationToken cancellationToken)
        => Spider.DefaultSend<GetTeamByIdQuery, TeamResponse>(new GetTeamByIdQuery(id), cancellationToken);

    /// <summary>
    /// Gets teams using a custom get-many query handler.
    /// </summary>
    [HttpGet]
    public Task<IEnumerable<TeamResponse>> GetMany([FromQuery] string city, CancellationToken cancellationToken)
        => Spider.DefaultSend<GetTeamsQuery, IEnumerable<TeamResponse>>(new GetTeamsQuery { City = city }, cancellationToken);
}
