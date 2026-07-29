namespace TurtlePath.Identifier
{
    /// <summary>
    /// Describes where an identifier value is generated.
    /// </summary>
    public enum CIdGenerationStrategy
    {
        /// <summary>
        /// The application creates identifier values.
        /// </summary>
        ClientGenerated,

        /// <summary>
        /// The backing store creates identifier values.
        /// </summary>
        StoreGenerated
    }
}
