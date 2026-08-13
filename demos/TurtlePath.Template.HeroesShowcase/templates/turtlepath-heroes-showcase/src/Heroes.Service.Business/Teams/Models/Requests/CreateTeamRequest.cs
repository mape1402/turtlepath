using Heroes.Service.Business.Teams.Models.Responses;
using Pelican.Mediator;

namespace Heroes.Service.Business.Teams.Models.Requests;

public sealed record CreateTeamRequest(
    string Name,
    string City,
    string Headquarters) : IRequest<TeamResponse>;
