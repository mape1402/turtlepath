using TurtlePath.Domain.Contracts;
using TurtlePath.Domain.Identifier;

namespace TurtlePath.Samples.Basic.Domain.Entities;

public sealed class LegacyInvoice : BaseEntity
{
    public CId CustomerId { get; set; } = CId.Empty;
    public decimal Amount { get; set; }
    public Customer Customer { get; set; }
}
