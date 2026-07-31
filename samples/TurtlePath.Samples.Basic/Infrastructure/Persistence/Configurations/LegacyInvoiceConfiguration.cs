using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Infrastructure.Persistence.Configurations;

public sealed class LegacyInvoiceConfiguration : IEntityTypeConfiguration<LegacyInvoice>
{
    public void Configure(EntityTypeBuilder<LegacyInvoice> builder)
    {
        builder.ToTable("LegacyInvoices");
        builder.Property(invoice => invoice.Amount).HasPrecision(18, 2);
        builder.HasOne(invoice => invoice.Customer)
            .WithMany(customer => customer.Invoices)
            .HasForeignKey(invoice => invoice.CustomerId);
    }
}
