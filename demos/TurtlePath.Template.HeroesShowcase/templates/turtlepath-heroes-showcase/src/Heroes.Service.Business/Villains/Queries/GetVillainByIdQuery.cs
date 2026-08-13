using Heroes.Service.Business.Villains.Models.Responses;
using Heroes.Service.Domain;
using TurtlePath.Domain.Identifier;
using TurtlePath.Queries;

namespace Heroes.Service.Business.Villains.Queries;

public sealed class GetVillainByIdQuery : GetByIdQuery<Villain, VillainResponse>
{
    public GetVillainByIdQuery(CId id) : base(id)
    {
    }
}
