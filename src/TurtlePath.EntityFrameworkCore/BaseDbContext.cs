namespace TurtlePath.EntityFrameworkCore
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using System.Reflection;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Domain.Identifier;

    /// <summary>
    /// Provides a reusable EF Core DbContext base for TurtlePath applications.
    /// </summary>
    public abstract class BaseDbContext : DbContext, IDbContext
    {
        private readonly TurtlePathDbContextOptions turtlePathOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDbContext"/> class.
        /// </summary>
        /// <param name="options">The options to configure the context.</param>
        /// <param name="turtlePathOptions">The TurtlePath conventions to apply to the model.</param>
        protected BaseDbContext(DbContextOptions options, TurtlePathDbContextOptions turtlePathOptions) : base(options)
        {
            this.turtlePathOptions = turtlePathOptions ?? TurtlePathDbContextOptions.Default;
        }

        /// <summary>
        /// Gets the assemblies used to discover entity configurations.
        /// </summary>
        protected virtual IEnumerable<Assembly> ConfigurationAssemblies
        {
            get { yield return GetType().Assembly; }
        }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            if (turtlePathOptions.ApplyConfigurations)
                ApplyConfigurations(builder);

            if (turtlePathOptions.ApplyBaseEntityConventions)
                ApplyBaseEntityConventions(builder);

            if (turtlePathOptions.ApplyCIdConverters)
                ApplyCIdConverters(builder);
        }

        private void ApplyConfigurations(ModelBuilder builder)
        {
            foreach (var assembly in GetConfigurationAssemblies().Where(assembly => assembly != null).Distinct())
                builder.ApplyConfigurationsFromAssembly(assembly);
        }

        private IEnumerable<Assembly> GetConfigurationAssemblies()
            => turtlePathOptions.ConfigurationAssemblies.Count > 0
                ? turtlePathOptions.ConfigurationAssemblies
                : ConfigurationAssemblies;

        private static void ApplyBaseEntityConventions(ModelBuilder builder)
        {
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

        private void ApplyCIdConverters(ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    var clrType = property.ClrType;
                    var underlyingType = Nullable.GetUnderlyingType(clrType);

                    if (clrType != typeof(CId) && underlyingType != typeof(CId))
                        continue;

                    var definition = GetCIdDefinition(entityType.ClrType, property.Name);
                    var converter = CreateCIdValueConverter(definition);

                    if (converter == null)
                        continue;

                    property.SetValueConverter(converter);

                    if (!string.IsNullOrWhiteSpace(definition.DatabaseColumnType))
                        property.SetColumnType(definition.DatabaseColumnType);
                }
            }
        }

        private CIdDefinition GetCIdDefinition(Type entityType, string propertyName)
        {
            if (turtlePathOptions.CIdDefinitions != null &&
                turtlePathOptions.CIdDefinitions.TryGet(entityType, propertyName, out var definition))
                return definition;

            return turtlePathOptions.CIdDefinition;
        }

        private static ValueConverter CreateCIdValueConverter(CIdDefinition definition)
        {
            if (definition?.HasDatabaseConversion != true)
                return null;

            var providerType = definition.DatabaseValueType ?? definition.ConvertToDatabase.ReturnType;
            var converterType = typeof(ValueConverter<,>).MakeGenericType(typeof(CId), providerType);

            return (ValueConverter)Activator.CreateInstance(
                converterType,
                definition.ConvertToDatabase,
                definition.ConvertFromDatabase,
                null);
        }
    }
}
