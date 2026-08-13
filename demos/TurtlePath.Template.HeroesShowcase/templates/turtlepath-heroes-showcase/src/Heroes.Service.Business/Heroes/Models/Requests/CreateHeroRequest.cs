using Heroes.Service.Business.Heroes.Models.Responses;
using Pelican.Mediator;
using TurtlePath.Domain.Identifier;

namespace Heroes.Service.Business.Heroes.Models.Requests;

public sealed record CreateHeroRequest(
    string Alias,
    string RealName,
    string City,
    int PowerLevel,
    CId TeamId) : IRequest<HeroResponse>;
