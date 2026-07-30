namespace TurtlePath.Domain.Identifier
{
    /// <summary>
    /// Default in-memory identifier definition registry.
    /// </summary>
    public sealed class CIdDefinitionRegistry : ICIdDefinitionRegistry, ICIdFactory
    {
        private readonly Dictionary<string, CIdDefinition> _definitions = new(StringComparer.Ordinal);

        /// <inheritdoc/>
        public void Register(CIdDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            _definitions[definition.Context] = definition;
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
    }
}

