using Microsoft.EntityFrameworkCore;
using TurtlePath.EntityFrameworkCore;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Infrastructure.Persistence;

public sealed class CommerceDbContext : BaseDbContext
{
    public CommerceDbContext(
        DbContextOptions<CommerceDbContext> options,
        TurtlePathDbContextOptions turtlePathOptions)
        : base(options, turtlePathOptions)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<TenantOrder> TenantOrders => Set<TenantOrder>();
    public DbSet<LegacyInvoice> LegacyInvoices => Set<LegacyInvoice>();
}
