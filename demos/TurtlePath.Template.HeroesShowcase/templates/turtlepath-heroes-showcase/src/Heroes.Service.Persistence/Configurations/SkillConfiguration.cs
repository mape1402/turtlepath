using Heroes.Service.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Heroes.Service.Persistence.Configurations;

public sealed class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("Skills");
        builder.HasKey(skill => skill.Id);
        builder.Property(skill => skill.Name).HasMaxLength(120).IsRequired();
        builder.Property(skill => skill.OwnerAlignment).HasConversion<string>().HasMaxLength(24);
        builder.HasOne(skill => skill.Hero)
            .WithMany(hero => hero.Skills)
            .HasForeignKey(skill => skill.HeroId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(skill => skill.Villain)
            .WithMany(villain => villain.Skills)
            .HasForeignKey(skill => skill.VillainId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
