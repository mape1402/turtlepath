using TurtlePath.Hooks;
using TurtlePath.Samples.Basic.Application.Requests;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.Hooks;

public sealed class NormalizeCustomerEmailHook : IBeforeValidationHook<CreateCustomerRequest, Customer>, IOrderedHook
{
    public int Order => 0;

    public ValueTask BeforeValidationAsync(
        CommandHookContext<CreateCustomerRequest, Customer> context,
        CancellationToken cancellationToken = default)
    {
        context.Entity.Email = context.Request.Email.Trim().ToLowerInvariant();

        return ValueTask.CompletedTask;
    }
}
