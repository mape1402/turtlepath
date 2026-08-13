using Heroes.Service.Business.Teams.Models.Responses;
using Heroes.Service.Domain;
using TurtlePath.Queries;

namespace Heroes.Service.Business.Teams.Queries;

public sealed class GetTeamsQuery : GetManyQuery<Team, TeamResponse>
{
    /// <summary>
    /// Gets or sets the city associated with the resource.
    /// </summary>
    public string City { get; init; } = string.Empty;
}
