namespace TurtlePath.Domain.Identifier
{
    /// <summary>
    /// Represents a named part of an opaque identifier.
    /// </summary>
    public readonly record struct CIdPart(string Name, object Value)
    {
        /// <summary>
        /// Creates a single unnamed identifier part.
        /// </summary>
        /// <param name="value">The underlying value.</param>
        /// <returns>The identifier part.</returns>
        public static CIdPart Single(object value)
            => new("Value", value);
    }
}

