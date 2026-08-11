using TurtlePath.Models.Responses;

namespace Heroes.Service.Business.Heroes.Models.Responses;

public sealed class HeroResponse : BaseResponse
{
    public string Alias { get; set; } = string.Empty;

    public string RealName { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public int PowerLevel { get; set; }

    public bool Active { get; set; }

    public string TeamName { get; set; } = string.Empty;
}
