using TurtlePath.Hooks;
using TurtlePath.Samples.Basic.Application.Requests;
using TurtlePath.Samples.Basic.Domain.Entities;
using TurtlePath.Samples.Basic.Infrastructure;

namespace TurtlePath.Samples.Basic.Application.Hooks;

public sealed class CustomerAuditAfterSaveHook : IAfterSaveHook<CreateCustomerRequest, Customer>
{
    private readonly SampleAuditLog auditLog;

    public CustomerAuditAfterSaveHook(SampleAuditLog auditLog)
    {
        this.auditLog = auditLog;
    }

    public ValueTask AfterSaveAsync(
        CommandHookContext<CreateCustomerRequest, Customer> context,
        CancellationToken cancellationToken = default)
    {
        auditLog.Add($"Customer saved: {context.Entity.Id} {context.Entity.Email}");

        return ValueTask.CompletedTask;
    }
}
