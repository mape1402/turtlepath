namespace TurtlePath.EntityFrameworkCore.Conventions
{
    using Microsoft.EntityFrameworkCore;
    using TurtlePath.Domain.Contracts;

    /// <summary>
    /// Applies default EF Core conventions for TurtlePath base entities.
    /// </summary>
    public sealed class BaseEntityModelConvention : ITurtlePathModelConvention
    {
        /// <inheritdoc/>
        public void Apply(ModelBuilder builder, TurtlePathDbContextOptions options)
        {
            if (options?.ApplyBaseEntityConventions != true)
                return;

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                    continue;

                var idProperty = entityType.FindProperty(nameof(BaseEntity.Id));

                if (idProperty == null)
                    continue;

                var primaryKey = entityType.FindPrimaryKey();
                var usesDefaultIdKey = primaryKey == null ||
                                       primaryKey.Properties.Count == 1 &&
                                       primaryKey.Properties[0].Name == nameof(BaseEntity.Id);

                if (!usesDefaultIdKey)
                    continue;

                builder.Entity(entityType.ClrType).HasKey(nameof(BaseEntity.Id));
                builder.Entity(entityType.ClrType).Property(nameof(BaseEntity.Id)).ValueGeneratedOnAdd();
            }
        }
    }
}
