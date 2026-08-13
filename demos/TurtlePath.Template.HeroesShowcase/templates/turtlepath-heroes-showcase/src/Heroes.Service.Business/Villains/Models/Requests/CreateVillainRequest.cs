using Heroes.Service.Business.Villains.Models.Responses;
using Heroes.Service.Domain.Enums;
using Pelican.Mediator;
using TurtlePath.Domain.Identifier;

namespace Heroes.Service.Business.Villains.Models.Requests;

public sealed record CreateVillainRequest(
    string Alias,
    string RealName,
    string Lair,
    int PowerLevel,
    ThreatLevel ThreatLevel,
    CId TeamId) : IRequest<VillainResponse>;
