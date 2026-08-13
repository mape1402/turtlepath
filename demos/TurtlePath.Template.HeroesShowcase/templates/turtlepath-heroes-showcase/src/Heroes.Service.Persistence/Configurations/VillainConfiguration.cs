using Heroes.Service.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Heroes.Service.Persistence.Configurations;

public sealed class VillainConfiguration : IEntityTypeConfiguration<Villain>
{
    public void Configure(EntityTypeBuilder<Villain> builder)
    {
        builder.ToTable("Villains");
        builder.HasKey(villain => villain.Id);
        builder.Property(villain => villain.Alias).HasMaxLength(120).IsRequired();
        builder.Property(villain => villain.RealName).HasMaxLength(160).IsRequired();
        builder.Property(villain => villain.Lair).HasMaxLength(160).IsRequired();
        builder.Property(villain => villain.ThreatLevel).HasConversion<string>().HasMaxLength(24);
        builder.HasOne(villain => villain.Team)
            .WithMany(team => team.Villains)
            .HasForeignKey(villain => villain.TeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
