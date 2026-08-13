using Heroes.Service.Business.Skills.Models.Responses;
using Pelican.Mediator;
using TurtlePath.Domain.Identifier;

namespace Heroes.Service.Business.Skills.Models.Requests;

public sealed record CreateVillainSkillRequest(
    CId VillainId,
    string Name,
    int Mastery) : IRequest<SkillResponse>;
