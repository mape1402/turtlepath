using Microsoft.EntityFrameworkCore;
using TurtlePath.Domain.Identifier;

namespace TurtlePath.Samples.Basic.Infrastructure.Persistence;

public sealed class LegacyInvoiceIdFactory
{
    private readonly CommerceDbContext dbContext;

    public LegacyInvoiceIdFactory(CommerceDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<CId> NewAsync(CancellationToken cancellationToken = default)
    {
        var invoices = await dbContext.LegacyInvoices
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var next = invoices.Count == 0
            ? 10_001
            : invoices.Max(invoice => invoice.Id.Cast<int>()) + 1;

        return CId.From(next);
    }
}
