using Pelican.Mediator;
using TurtlePath.Commands;
using TurtlePath.Models.Requests;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.Requests;

public sealed class PatchCustomerEmailRequest : BaseRequest, IRequest<CustomerResponse>, IPatchAction<Customer>
{
    public string Email { get; set; } = string.Empty;

    public ValueTask PatchAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        entity.Email = Email.Trim().ToLowerInvariant();
        return ValueTask.CompletedTask;
    }
}
