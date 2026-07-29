using TurtlePath.Identifier;

namespace TurtlePath.Contracts
{
    /// <summary>
    /// Provides a base implementation for entities with a <see cref="CId"/> identifier.
    /// </summary>
    public abstract class BaseEntity : IEntity<CId>
    {
        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        public CId Id { get; set; }
    }
}
