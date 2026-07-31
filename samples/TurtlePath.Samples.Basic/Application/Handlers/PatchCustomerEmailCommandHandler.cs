using TurtlePath.Commands;
using TurtlePath.Samples.Basic.Application.Requests;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.Handlers;

public sealed class PatchCustomerEmailCommandHandler : PatchCommandHandler<PatchCustomerEmailRequest, CustomerResponse, Customer>
{
    public PatchCustomerEmailCommandHandler(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    protected override bool ValidateRequest => true;

    protected override ValueTask PatchEntityAsync(
        PatchCustomerEmailRequest request,
        Customer entity,
        CancellationToken cancellationToken)
    {
        entity.Email = request.Email.Trim().ToLowerInvariant();
        return ValueTask.CompletedTask;
    }

    protected override ValueTask<CustomerResponse> BuildResponseAsync(
        PatchCustomerEmailRequest request,
        Customer entity,
        CancellationToken cancellationToken)
        => MapperAdapter.MapAsync<Customer, CustomerResponse>(entity, cancellationToken);
}
