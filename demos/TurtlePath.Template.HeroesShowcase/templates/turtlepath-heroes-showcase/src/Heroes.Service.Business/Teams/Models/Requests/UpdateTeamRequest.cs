using Heroes.Service.Business.Teams.Models.Responses;
using Pelican.Mediator;
using TurtlePath.Models.Requests;

namespace Heroes.Service.Business.Teams.Models.Requests;

public sealed class UpdateTeamRequest : BaseRequest, IRequest<TeamResponse>
{
    /// <summary>
    /// Gets or sets the display name of the resource.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the city associated with the resource.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the team headquarters.
    /// </summary>
    public string Headquarters { get; set; } = string.Empty;
}
