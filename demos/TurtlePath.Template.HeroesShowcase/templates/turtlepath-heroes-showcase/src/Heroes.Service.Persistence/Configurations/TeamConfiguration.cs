using Heroes.Service.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Heroes.Service.Persistence.Configurations;

public sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");
        builder.HasKey(team => team.Id);
        builder.Property(team => team.Name).HasMaxLength(120).IsRequired();
        builder.Property(team => team.City).HasMaxLength(80).IsRequired();
        builder.Property(team => team.Headquarters).HasMaxLength(160).IsRequired();
    }
}
