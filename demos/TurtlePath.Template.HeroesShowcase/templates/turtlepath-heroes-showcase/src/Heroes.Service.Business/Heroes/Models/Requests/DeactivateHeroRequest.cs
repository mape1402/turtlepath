using Heroes.Service.Domain;
using Pelican.Mediator;
using TurtlePath.Commands;
using TurtlePath.Models.Requests;

namespace Heroes.Service.Business.Heroes.Models.Requests;

public sealed class DeactivateHeroRequest : BaseRequest, IRequest, IPatchAction<Hero>
{
    public ValueTask PatchAsync(Hero entity, CancellationToken cancellationToken = default)
    {
        entity.Active = false;
        return ValueTask.CompletedTask;
    }
}
