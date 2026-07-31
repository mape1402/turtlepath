using TurtlePath.Hooks;
using TurtlePath.Samples.Basic.Application.Requests;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.Hooks;

public sealed class NormalizeCustomerEmailAfterMapHook : IAfterMapHook<CreateCustomerRequest, Customer>, IOrderedHook
{
    public int Order => 0;

    public ValueTask AfterMapAsync(
        CommandHookContext<CreateCustomerRequest, Customer> context,
        CancellationToken cancellationToken = default)
    {
        context.Entity.Email = context.Entity.Email.Trim().ToLowerInvariant();

        return ValueTask.CompletedTask;
    }
}
