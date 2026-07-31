using TurtlePath.Commands;
using TurtlePath.Domain.Identifier;
using TurtlePath.Samples.Basic.Application.Requests;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.Handlers;

public sealed class CreateTenantOrderCommandHandler : CreateCommandHandler<CreateTenantOrderRequest, TenantOrderResponse, TenantOrder>
{
    private readonly ICIdDefinitionRegistry idDefinitions;
    private CId legacyInvoiceId = CId.Empty;

    public CreateTenantOrderCommandHandler(
        IServiceProvider serviceProvider,
        ICIdDefinitionRegistry idDefinitions)
        : base(serviceProvider)
    {
        this.idDefinitions = idDefinitions;
    }

    protected override async Task SaveEntityAsync(
        CreateTenantOrderRequest request,
        TenantOrder entity,
        CancellationToken cancellationToken)
    {
        await base.SaveEntityAsync(request, entity, cancellationToken);

        var invoice = new LegacyInvoice
        {
            Id = idDefinitions.Get(typeof(LegacyInvoice)).Factory(),
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
