using TurtlePath.Domain.Contracts;
using TurtlePath.Domain.Identifier;
using Sieve.Attributes;

namespace TurtlePath.Samples.Basic.Domain.Entities;

public sealed class Customer : BaseEntity
{
    [Sieve(CanFilter = true, CanSort = true)]
    public string Name { get; set; } = string.Empty;

    [Sieve(CanFilter = true, CanSort = true)]
    public string Email { get; set; } = string.Empty;

    public List<TenantOrder> Orders { get; } = [];
    public List<LegacyInvoice> Invoices { get; } = [];
}
