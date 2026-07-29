namespace TurtlePath.Core.Services
{
    using TurtlePath.Contracts;

    /// <summary>
    /// Defines an abstraction for storage operations on entities, including save, update, delete, and retrieval.
    /// </summary>
    public interface IStorageWriterAdapter
    {
        /// <summary>
        /// Adds an entity to the current storage unit of work.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to add.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        ValueTask AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : BaseEntity;

        /// <summary>
        /// Adds entities to the current storage unit of work.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <param name="entities">The entities to add.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : BaseEntity;

        /// <summary>
        /// Marks an entity as updated in the current storage unit of work.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to update.</param>
        void Update<TEntity>(TEntity entity) where TEntity : BaseEntity;

        /// <summary>
        /// Marks entities as updated in the current storage unit of work.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <param name="entities">The entities to update.</param>
        void UpdateRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity;

        /// <summary>
        /// Removes an entity from the current storage unit of work.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to remove.</param>
        void Remove<TEntity>(TEntity entity) where TEntity : BaseEntity;

        /// <summary>
        /// Removes entities from the current storage unit of work.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities.</typeparam>
        /// <param name="entities">The entities to remove.</param>
        void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity;

        /// <summary>
        /// Persists all pending storage changes.
        /// </summary>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>The number of affected entries.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves the entity to the storage.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to save.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Obsolete("Use AddAsync followed by SaveChangesAsync.")]
        Task SaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : BaseEntity;
       
        /// <summary>
        /// Updates the entity in the storage.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to update.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Obsolete("Use Update when the entity is detached, then SaveChangesAsync.")]
        Task UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : BaseEntity;

        /// <summary>
        /// Deletes an entity from the storage.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to delete.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Obsolete("Use Remove followed by SaveChangesAsync.")]
        Task DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : BaseEntity;
    }
}
