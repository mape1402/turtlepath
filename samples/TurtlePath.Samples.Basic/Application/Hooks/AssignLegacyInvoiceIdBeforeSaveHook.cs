using TurtlePath.Hooks;
using TurtlePath.Samples.Basic.Application.Requests;
using TurtlePath.Samples.Basic.Domain.Entities;
using TurtlePath.Samples.Basic.Infrastructure.Persistence;

namespace TurtlePath.Samples.Basic.Application.Hooks;

public sealed class AssignLegacyInvoiceIdBeforeSaveHook : IBeforeSaveHook<CreateLegacyInvoiceRequest, LegacyInvoice>
{
    private readonly LegacyInvoiceIdFactory idFactory;

    public AssignLegacyInvoiceIdBeforeSaveHook(LegacyInvoiceIdFactory idFactory)
    {
        this.idFactory = idFactory;
    }

    public async ValueTask BeforeSaveAsync(
        CommandHookContext<CreateLegacyInvoiceRequest, LegacyInvoice> context,
        CancellationToken cancellationToken = default)
    {
        if (context.Entity.Id.IsEmpty)
            context.Entity.Id = await idFactory.NewAsync(cancellationToken);
    }
}
