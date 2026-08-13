using Heroes.Service.Business.Teams.Models.Responses;
using Heroes.Service.Domain;
using TurtlePath.Queries;

namespace Heroes.Service.Business.Teams.Queries;

public sealed class GetTeamsQuery : GetManyQuery<Team, TeamResponse>
{
    public string City { get; init; } = string.Empty;
}
