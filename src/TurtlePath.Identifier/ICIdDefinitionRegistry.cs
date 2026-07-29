namespace TurtlePath.Identifier
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
    }
}
