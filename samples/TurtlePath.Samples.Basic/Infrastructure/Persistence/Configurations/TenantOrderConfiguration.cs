using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Infrastructure.Persistence.Configurations;

public sealed class TenantOrderConfiguration : IEntityTypeConfiguration<TenantOrder>
{
    public void Configure(EntityTypeBuilder<TenantOrder> builder)
    {
        builder.ToTable("TenantOrders");
        builder.Property(order => order.Id);
        builder.Property(order => order.CustomerId);
        builder.Property(order => order.Total).HasPrecision(18, 2);
        builder.HasOne(order => order.Customer)
            .WithMany(customer => customer.Orders)
            .HasForeignKey(order => order.CustomerId);
    }
}
