using TurtlePath.Domain.Identifier;
using TurtlePath.Hooks;
using TurtlePath.Samples.Basic.Application.Requests;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.Hooks;

public sealed class AssignCustomerIdBeforeSaveHook : IBeforeSaveHook<CreateCustomerRequest, Customer>, IOrderedHook
{
    private readonly ICIdFactory idFactory;

    public AssignCustomerIdBeforeSaveHook(ICIdFactory idFactory)
    {
        this.idFactory = idFactory;
    }

    public int Order => 10;

    public ValueTask BeforeSaveAsync(
        CommandHookContext<CreateCustomerRequest, Customer> context,
        CancellationToken cancellationToken = default)
    {
        if (context.Entity.Id.IsEmpty)
            context.Entity.Id = idFactory.New();

        return ValueTask.CompletedTask;
    }
}
