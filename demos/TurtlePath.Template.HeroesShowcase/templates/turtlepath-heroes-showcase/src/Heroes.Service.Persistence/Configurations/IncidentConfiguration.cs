using Heroes.Service.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Heroes.Service.Persistence.Configurations;

public sealed class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("Incidents");
        builder.HasKey(incident => incident.Id);
        builder.Property(incident => incident.Title).HasMaxLength(180).IsRequired();
        builder.Property(incident => incident.City).HasMaxLength(80).IsRequired();
        builder.Property(incident => incident.ThreatLevel).HasConversion<string>().HasMaxLength(24);
        builder.Property(incident => incident.Status).HasConversion<string>().HasMaxLength(24);
        builder.HasOne(incident => incident.AssignedHero)
            .WithMany()
            .HasForeignKey(incident => incident.AssignedHeroId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(incident => incident.SuspectedVillain)
            .WithMany()
            .HasForeignKey(incident => incident.SuspectedVillainId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
