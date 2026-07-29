namespace TurtlePath.Contracts
{
    /// <summary>
    /// Defines a non-generic entity marker interface.
    /// </summary>
    public interface IEntity { }

    /// <summary>
    /// Defines an entity with a strongly-typed identifier.
    /// </summary>
    /// <typeparam name="TKey">The type of the identifier.</typeparam>
    public interface IEntity<TKey> : IEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        TKey Id { get; set; }
    }
}
