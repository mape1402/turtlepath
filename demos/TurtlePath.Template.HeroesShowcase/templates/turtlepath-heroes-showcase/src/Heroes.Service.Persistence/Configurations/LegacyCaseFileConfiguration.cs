using Heroes.Service.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Heroes.Service.Persistence.Configurations;

/// <summary>
/// Maps legacy case files to a table with an integer primary key behind TurtlePath CId.
/// </summary>
public sealed class LegacyCaseFileConfiguration : IEntityTypeConfiguration<LegacyCaseFile>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LegacyCaseFile> builder)
    {
        builder.ToTable("LegacyCaseFiles");
        builder.HasKey(file => file.Id);
        builder.Property(file => file.Id).ValueGeneratedNever();
        builder.Property(file => file.ExternalNumber).HasMaxLength(80).IsRequired();
        builder.Property(file => file.City).HasMaxLength(80).IsRequired();
    }
}
