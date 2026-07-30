using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurtlePath.Contracts;

namespace TurtlePath.EntityFrameworkCore
{
    /// <summary>
    /// Provides a base configuration for entities using <see cref="BaseEntity"/> identifiers.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : BaseEntity
    {
        /// <summary>
        /// Configures the entity type.
        /// </summary>
        /// <param name="builder">The builder used to configure the entity.</param>
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();
        }
    }
}
