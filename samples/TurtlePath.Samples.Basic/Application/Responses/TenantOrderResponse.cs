using TurtlePath.Domain.Identifier;
using TurtlePath.Models.Responses;

namespace TurtlePath.Samples.Basic.Application.Responses;

public sealed class TenantOrderResponse : BaseResponse
{
    public CId CustomerId { get; set; } = CId.Empty;
    public decimal Total { get; set; }
    public CId LegacyInvoiceId { get; set; } = CId.Empty;
}
