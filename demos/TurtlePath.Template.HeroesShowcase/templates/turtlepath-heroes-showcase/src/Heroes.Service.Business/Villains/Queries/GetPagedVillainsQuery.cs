using Heroes.Service.Business.Villains.Models.Responses;
using Heroes.Service.Domain;
using TurtlePath.Queries;

namespace Heroes.Service.Business.Villains.Queries;

public sealed class GetPagedVillainsQuery : GetPagedInfoQuery<Villain, VillainResponse>
{
    public GetPagedVillainsQuery(PagedSettings pagedSettings) : base(pagedSettings ?? new PagedSettings())
    {
    }
}
