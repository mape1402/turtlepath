using Heroes.Service.Domain.Enums;
using TurtlePath.Models.Responses;

namespace Heroes.Service.Business.Villains.Models.Responses;

public sealed class VillainResponse : BaseResponse
{
    public string Alias { get; set; } = string.Empty;

    public string RealName { get; set; } = string.Empty;

    public string Lair { get; set; } = string.Empty;

    public int PowerLevel { get; set; }

    public ThreatLevel ThreatLevel { get; set; }

    public bool Captured { get; set; }

    public string TeamName { get; set; } = string.Empty;
}
