using Heroes.Service.Business.Skills.Models.Responses;
using Pelican.Mediator;
using TurtlePath.Domain.Identifier;

namespace Heroes.Service.Business.Skills.Models.Requests;

public sealed record CreateHeroSkillRequest(
    CId HeroId,
    string Name,
    int Mastery) : IRequest<SkillResponse>;
