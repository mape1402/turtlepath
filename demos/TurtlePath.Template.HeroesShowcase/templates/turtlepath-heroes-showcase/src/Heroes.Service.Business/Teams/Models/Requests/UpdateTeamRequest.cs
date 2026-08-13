using Heroes.Service.Business.Teams.Models.Responses;
using Pelican.Mediator;
using TurtlePath.Models.Requests;

namespace Heroes.Service.Business.Teams.Models.Requests;

public sealed class UpdateTeamRequest : BaseRequest, IRequest<TeamResponse>
{
    public string Name { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Headquarters { get; set; } = string.Empty;
}
