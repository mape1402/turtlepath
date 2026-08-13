using Heroes.Service.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Heroes.Service.Persistence.Configurations;

public sealed class HeroConfiguration : IEntityTypeConfiguration<Hero>
{
    public void Configure(EntityTypeBuilder<Hero> builder)
    {
        builder.ToTable("Heroes");
        builder.HasKey(hero => hero.Id);
        builder.Property(hero => hero.Alias).HasMaxLength(120).IsRequired();
        builder.Property(hero => hero.RealName).HasMaxLength(160).IsRequired();
        builder.Property(hero => hero.City).HasMaxLength(80).IsRequired();
        builder.HasOne(hero => hero.Team)
            .WithMany(team => team.Heroes)
            .HasForeignKey(hero => hero.TeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
