using Heroes.Service.Business.Villains.Models.Responses;
using Heroes.Service.Domain;
using Pelican.Mediator;
using TurtlePath.Commands;
using TurtlePath.Models.Requests;

namespace Heroes.Service.Business.Villains.Models.Requests;

public sealed class CaptureVillainRequest : BaseRequest, IRequest<VillainResponse>, IPatchAction<Villain>
{
    public ValueTask PatchAsync(Villain entity, CancellationToken cancellationToken = default)
    {
        entity.Captured = true;
        return ValueTask.CompletedTask;
    }
}
