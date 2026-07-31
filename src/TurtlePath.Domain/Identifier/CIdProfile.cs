namespace TurtlePath.Domain.Identifier
{
    /// <summary>
    /// Base class for CId definition profiles.
    /// </summary>
    public abstract class CIdProfile : ICIdProfile
    {
        /// <inheritdoc/>
        public abstract void Configure(CIdProfileBuilder builder);
    }
}
