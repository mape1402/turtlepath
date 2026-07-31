using TurtlePath.Commands;
using TurtlePath.Samples.Basic.Application.Requests;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.Handlers;

public sealed class DeleteTenantOrderCommandHandler : DeleteCommandHandler<DeleteTenantOrderRequest, DeletedResourceResponse, TenantOrder>
{
    public DeleteTenantOrderCommandHandler(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    protected override ValueTask<DeletedResourceResponse> BuildResponseAsync(
        DeleteTenantOrderRequest request,
        TenantOrder entity,
        CancellationToken cancellationToken)
        => ValueTask.FromResult(new DeletedResourceResponse
        {
            Id = entity.Id,
            Resource = nameof(TenantOrder)
        });
}
