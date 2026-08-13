using Heroes.Service.Business.Villains.Models.Responses;
using Heroes.Service.Domain.Enums;
using Pelican.Mediator;
using TurtlePath.Domain.Identifier;
using TurtlePath.Models.Requests;

namespace Heroes.Service.Business.Villains.Models.Requests;

public sealed class UpdateVillainRequest : BaseRequest, IRequest<VillainResponse>
{
    public string Alias { get; set; } = string.Empty;

    public string RealName { get; set; } = string.Empty;

    public string Lair { get; set; } = string.Empty;

    public int PowerLevel { get; set; }

    public ThreatLevel ThreatLevel { get; set; }

    public CId TeamId { get; set; }
}
