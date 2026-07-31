namespace TurtlePath.Domain.Identifier
{
    /// <summary>
    /// Default in-memory identifier definition registry.
    /// </summary>
    public sealed class CIdDefinitionRegistry : ICIdDefinitionRegistry, ICIdFactory
    {
        private readonly Dictionary<string, CIdDefinition> _definitions = new(StringComparer.Ordinal);
        private readonly Dictionary<(Type EntityType, string PropertyName), CIdDefinition> _entityDefinitions = new();

        /// <inheritdoc/>
        public void Register(CIdDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            _definitions[definition.Context] = definition;

            if (!definition.IsDefault)
                _entityDefinitions[(definition.EntityType, definition.PropertyName)] = definition;
        }

        /// <inheritdoc/>
        public CIdDefinition Get(string context = CIdDefinition.DefaultContext)
        {
            if (_definitions.TryGetValue(context, out var definition))
                return definition;

            throw new InvalidOperationException($"No CId definition is registered for context '{context}'.");
        }

        /// <inheritdoc/>
        public CId New(string context = CIdDefinition.DefaultContext)
            => Get(context).Factory();

        /// <inheritdoc/>
        public CIdDefinition Get(Type entityType, string propertyName = CIdDefinition.DefaultPropertyName)
        {
            if (TryGet(entityType, propertyName, out var definition))
                return definition;

            throw new InvalidOperationException($"No CId definition is registered for '{entityType?.FullName}.{propertyName}'.");
        }

        /// <inheritdoc/>
        public bool TryGet(Type entityType, string propertyName, out CIdDefinition definition)
        {
            if (entityType != null &&
                _entityDefinitions.TryGetValue((entityType, propertyName ?? CIdDefinition.DefaultPropertyName), out definition))
                return true;

            return _definitions.TryGetValue(CIdDefinition.DefaultContext, out definition);
        }
    }
}

