namespace TurtlePath.Testing.Persistence
{
    using TurtlePath.Domain.Contracts;

    /// <summary>
    /// Assertion-friendly access to entities of one type in the in-memory test storage.
    /// </summary>
    public sealed class InMemoryEntitySet<TEntity>
        where TEntity : class, IEntity
    {
        private readonly InMemoryTurtlePathStorage storage;

        internal InMemoryEntitySet(InMemoryTurtlePathStorage storage)
        {
            this.storage = storage;
        }

        /// <summary>
        /// Gets the current entities.
        /// </summary>
        public IReadOnlyList<TEntity> Items => storage.Entities<TEntity>();

        /// <summary>
        /// Adds entities to the store.
        /// </summary>
        public InMemoryEntitySet<TEntity> Seed(params TEntity[] entities)
        {
            storage.Seed(entities);
            return this;
        }

        /// <summary>
        /// Returns the first matching entity or null.
        /// </summary>
        public TEntity FirstOrDefault(Func<TEntity, bool> predicate)
            => Items.FirstOrDefault(predicate);

        /// <summary>
        /// Returns true when at least one entity matches the predicate.
        /// </summary>
        public bool Contains(Func<TEntity, bool> predicate)
            => Items.Any(predicate);
    }
}
