using TurtlePath.Domain.Identifier;
using TurtlePath.Models.Responses;

namespace TurtlePath.Samples.Basic.Application.Responses;

public sealed class LegacyInvoiceResponse : BaseResponse
{
    public CId CustomerId { get; set; } = CId.Empty;
    public decimal Amount { get; set; }
}
