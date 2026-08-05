using TurtlePath.Domain.Identifier;
using TurtlePath.Queries;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.Queries;

public sealed class GetLegacyInvoiceByIdQuery : GetByIdQuery<LegacyInvoice, LegacyInvoiceResponse>
{
    public GetLegacyInvoiceByIdQuery(CId id) : base(id)
    {
    }
}
