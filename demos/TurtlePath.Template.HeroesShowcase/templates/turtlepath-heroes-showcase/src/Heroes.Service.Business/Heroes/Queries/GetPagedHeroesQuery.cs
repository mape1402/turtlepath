using Heroes.Service.Business.Heroes.Models.Responses;
using Heroes.Service.Domain;
using TurtlePath.Domain.Identifier;
using TurtlePath.Queries;

namespace Heroes.Service.Business.Heroes.Queries;

public sealed class GetPagedHeroesQuery : GetPagedInfoQuery<Hero, HeroResponse>
{
    public GetPagedHeroesQuery(PagedSettings pagedSettings) : base(pagedSettings ?? new PagedSettings())
    {
    }

    public CId? TeamId { get; set; }
}
