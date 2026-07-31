namespace TurtlePath.EntityFrameworkCore.Conventions
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using TurtlePath.Domain.Identifier;

    /// <summary>
    /// Applies EF Core value converters for scalar TurtlePath CId properties.
    /// </summary>
    public sealed class CIdModelConvention : ITurtlePathModelConvention
    {
        /// <inheritdoc/>
        public void Apply(ModelBuilder builder, TurtlePathDbContextOptions options)
        {
            if (options?.ApplyCIdConverters != true)
                return;

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    var clrType = property.ClrType;
                    var underlyingType = Nullable.GetUnderlyingType(clrType);

                    if (clrType != typeof(CId) && underlyingType != typeof(CId))
                        continue;

                    var definition = GetCIdDefinition(options, entityType.ClrType, property.Name);
                    var converter = CreateCIdValueConverter(definition);

                    if (converter == null)
                        continue;

                    property.SetValueConverter(converter);

                    if (!string.IsNullOrWhiteSpace(definition.DatabaseColumnType))
                        property.SetColumnType(definition.DatabaseColumnType);
                }
            }
        }

        private static CIdDefinition GetCIdDefinition(TurtlePathDbContextOptions options, Type entityType, string propertyName)
        {
            if (options.CIdDefinitions != null &&
                options.CIdDefinitions.TryGet(entityType, propertyName, out var definition))
                return definition;

            return options.CIdDefinition;
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
