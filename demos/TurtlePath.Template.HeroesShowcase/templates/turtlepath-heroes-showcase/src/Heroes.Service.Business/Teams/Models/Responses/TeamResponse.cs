using TurtlePath.Models.Responses;

namespace Heroes.Service.Business.Teams.Models.Responses;

public sealed class TeamResponse : BaseResponse
{
    public string Name { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Headquarters { get; set; } = string.Empty;

    public int Reputation { get; set; }
}
