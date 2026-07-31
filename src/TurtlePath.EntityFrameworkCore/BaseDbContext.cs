namespace TurtlePath.EntityFrameworkCore
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using System.Reflection;
    using TurtlePath.Domain.Identifier;

    /// <summary>
    /// Provides a reusable EF Core DbContext base for TurtlePath applications.
    /// </summary>
    public abstract class BaseDbContext : DbContext, IDbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDbContext"/> class.
        /// </summary>
        /// <param name="options">The options to configure the context.</param>
        protected BaseDbContext(DbContextOptions options) : base(options)
        {
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
            ApplyConfigurations(builder);
            ApplyCIdConverters(builder);
        }

        private void ApplyConfigurations(ModelBuilder builder)
        {
            foreach (var assembly in ConfigurationAssemblies.Where(assembly => assembly != null).Distinct())
                builder.ApplyConfigurationsFromAssembly(assembly);
        }

        private static void ApplyCIdConverters(ModelBuilder builder)
        {
            var converter = CreateCIdValueConverter();

            if (converter == null)
                return;

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    var clrType = property.ClrType;
                    var underlyingType = Nullable.GetUnderlyingType(clrType);

                    if (clrType != typeof(CId) && underlyingType != typeof(CId))
                        continue;

                    property.SetValueConverter(converter);

                    if (CIdMetadata.HasDbType)
                        property.SetColumnType(CIdMetadata.DbType);
                }
            }
        }

        private static ValueConverter CreateCIdValueConverter()
        {
            if (CIdMetadata.ConvertToDb == null || CIdMetadata.ConvertFromDb == null)
                return null;

            var providerType = CIdMetadata.ConvertToDb.ReturnType;
            var converterType = typeof(ValueConverter<,>).MakeGenericType(typeof(CId), providerType);

            return (ValueConverter)Activator.CreateInstance(
                converterType,
                CIdMetadata.ConvertToDb,
                CIdMetadata.ConvertFromDb,
                null);
        }
    }
}
