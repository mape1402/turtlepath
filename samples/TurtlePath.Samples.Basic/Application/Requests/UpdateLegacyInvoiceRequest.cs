using Pelican.Mediator;
using TurtlePath.Domain.Identifier;
using TurtlePath.Models.Requests;
using TurtlePath.Samples.Basic.Application.Responses;

namespace TurtlePath.Samples.Basic.Application.Requests;

public sealed class UpdateLegacyInvoiceRequest : BaseRequest, IRequest<LegacyInvoiceResponse>
{
    public CId CustomerId { get; set; } = CId.Empty;
    public decimal Amount { get; set; }
}
