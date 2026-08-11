using Heroes.Service.Business.Heroes.Models.Responses;
using Heroes.Service.Domain;
using TurtlePath.Domain.Identifier;
using TurtlePath.Queries;

namespace Heroes.Service.Business.Heroes.Queries;

public sealed class GetHeroByIdQuery : GetByIdQuery<Hero, HeroResponse>
{
    public GetHeroByIdQuery(CId id) : base(id)
    {
    }
}
