using TurtlePath.Domain.Contracts;
using TurtlePath.Domain.Identifier;

namespace TurtlePath.Samples.Basic.Domain.Entities;

public sealed class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<TenantOrder> Orders { get; } = [];
    public List<LegacyInvoice> Invoices { get; } = [];
}
