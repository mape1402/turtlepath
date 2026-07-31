namespace TurtlePath.EntityFrameworkCore
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TurtlePath.Domain.Contracts;

    /// <summary>
    /// Provides a compatibility base for entity configurations.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
        where TEntity : BaseEntity
    {
        /// <summary>
        /// Configures the entity type. Base TurtlePath entity conventions are applied by <see cref="BaseDbContext"/>.
        /// </summary>
        /// <param name="builder">The builder used to configure the entity.</param>
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
        }
    }
}
