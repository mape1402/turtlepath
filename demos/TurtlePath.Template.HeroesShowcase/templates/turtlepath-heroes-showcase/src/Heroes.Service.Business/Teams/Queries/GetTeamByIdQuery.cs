using Heroes.Service.Business.Teams.Models.Responses;
using Heroes.Service.Domain;
using TurtlePath.Domain.Identifier;
using TurtlePath.Queries;

namespace Heroes.Service.Business.Teams.Queries;

public sealed class GetTeamByIdQuery : GetByIdQuery<Team, TeamResponse>
{
    public GetTeamByIdQuery(CId id) : base(id)
    {
    }
}
