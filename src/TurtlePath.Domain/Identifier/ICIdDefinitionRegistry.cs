namespace TurtlePath.Domain.Identifier
{
    /// <summary>
    /// Stores identifier definitions by context.
    /// </summary>
    public interface ICIdDefinitionRegistry
    {
        /// <summary>
        /// Registers an identifier definition.
        /// </summary>
        /// <param name="definition">The definition to register.</param>
        void Register(CIdDefinition definition);

        /// <summary>
        /// Gets a definition by context.
        /// </summary>
        /// <param name="context">The definition context.</param>
        /// <returns>The matching definition.</returns>
        CIdDefinition Get(string context = CIdDefinition.DefaultContext);

        /// <summary>
        /// Gets a definition for an entity property, falling back to the default definition.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <param name="propertyName">The identifier property name.</param>
        /// <returns>The matching definition.</returns>
        CIdDefinition Get(Type entityType, string propertyName = CIdDefinition.DefaultPropertyName);

        /// <summary>
        /// Attempts to get a definition for an entity property, falling back to the default definition.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <param name="propertyName">The identifier property name.</param>
        /// <param name="definition">The matching definition.</param>
        /// <returns>True when a definition exists; otherwise, false.</returns>
        bool TryGet(Type entityType, string propertyName, out CIdDefinition definition);
    }
}

