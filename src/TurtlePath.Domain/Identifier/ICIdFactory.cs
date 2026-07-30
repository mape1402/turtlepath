namespace TurtlePath.Domain.Identifier
{
    /// <summary>
    /// Creates opaque identifiers for configured contexts.
    /// </summary>
    public interface ICIdFactory
    {
        /// <summary>
        /// Creates a new identifier for the specified context.
        /// </summary>
        /// <param name="context">The definition context.</param>
        /// <returns>The new identifier.</returns>
        CId New(string context = CIdDefinition.DefaultContext);
    }
}

