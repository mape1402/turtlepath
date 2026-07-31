using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Infrastructure.Persistence.Configurations;

public sealed class LegacyShipmentConfiguration : IEntityTypeConfiguration<LegacyShipment>
{
    public void Configure(EntityTypeBuilder<LegacyShipment> builder)
    {
        builder.ToTable("LegacyShipments");
        builder.HasKey(shipment => shipment.Id);
        builder.Property(shipment => shipment.Id).ValueGeneratedNever();
        builder.Property(shipment => shipment.Carrier).HasMaxLength(80).IsRequired();
        builder.Property(shipment => shipment.TrackingNumber).HasMaxLength(80).IsRequired();
    }
}
