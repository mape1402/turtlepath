using TurtlePath.Domain.Contracts;
using TurtlePath.Domain.Identifier;

namespace TurtlePath.Samples.Basic.Domain.Entities;

public sealed class TenantOrder : BaseEntity
{
    public CId CustomerId { get; set; } = CId.Empty;
    public decimal Total { get; set; }
    public Customer Customer { get; set; }
}
