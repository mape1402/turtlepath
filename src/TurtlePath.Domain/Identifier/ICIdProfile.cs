namespace TurtlePath.Domain.Identifier
{
    /// <summary>
    /// Defines a group of CId definitions for an application or persistence boundary.
    /// </summary>
    public interface ICIdProfile
    {
        /// <summary>
        /// Configures CId definitions.
        /// </summary>
        /// <param name="builder">The CId profile builder.</param>
        void Configure(CIdProfileBuilder builder);
    }
}
