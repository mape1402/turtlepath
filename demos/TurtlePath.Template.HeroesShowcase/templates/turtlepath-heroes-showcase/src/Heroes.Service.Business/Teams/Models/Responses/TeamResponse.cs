using TurtlePath.Models.Responses;

namespace Heroes.Service.Business.Teams.Models.Responses;

public sealed class TeamResponse : BaseResponse
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

    /// <summary>
    /// Gets or sets the calculated team reputation score.
    /// </summary>
    public int Reputation { get; set; }
}
