using TurtlePath.Commands;
using TurtlePath.Domain.Identifier;
using TurtlePath.Samples.Basic.Application.Requests;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;
using TurtlePath.Samples.Basic.Infrastructure.Persistence;

namespace TurtlePath.Samples.Basic.Application.Handlers;

public sealed class CreateTenantOrderCommandHandler : CreateCommandHandler<CreateTenantOrderRequest, TenantOrderResponse, TenantOrder>
{
    private readonly LegacyInvoiceIdFactory legacyInvoiceIdFactory;
    private CId legacyInvoiceId = CId.Empty;

    public CreateTenantOrderCommandHandler(
        IServiceProvider serviceProvider,
        LegacyInvoiceIdFactory legacyInvoiceIdFactory)
        : base(serviceProvider)
    {
        this.legacyInvoiceIdFactory = legacyInvoiceIdFactory;
    }

    protected override async Task SaveEntityAsync(
        CreateTenantOrderRequest request,
        TenantOrder entity,
        CancellationToken cancellationToken)
    {
        await base.SaveEntityAsync(request, entity, cancellationToken);

        var invoice = new LegacyInvoice
        {
            Id = await legacyInvoiceIdFactory.NewAsync(cancellationToken),
            CustomerId = request.CustomerId,
            Amount = request.Total
        };

        legacyInvoiceId = invoice.Id;

        await StorageWriterAdapter.AddAsync(invoice, cancellationToken);
        await StorageWriterAdapter.SaveChangesAsync(cancellationToken);
    }

    protected override async ValueTask<TenantOrderResponse> MapToResponseAsync(
        CreateTenantOrderRequest request,
        TenantOrder entity,
        CancellationToken cancellationToken)
    {
        var response = await base.MapToResponseAsync(request, entity, cancellationToken);
        response.LegacyInvoiceId = legacyInvoiceId;

        return response;
    }
}
