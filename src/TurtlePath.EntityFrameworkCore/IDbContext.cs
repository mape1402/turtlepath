#nullable enable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

namespace TurtlePath.EntityFrameworkCore
{
    /// <summary>
    /// Represents an abstraction for the application's database context, providing methods for querying and saving data.
    /// </summary>
    public interface IDbContext : IDisposable, IInfrastructure<IServiceProvider>
    {
        /// <summary>
        /// Provides access to database related information and operations.
        /// </summary>
        DatabaseFacade Database { get; }

        /// <summary>
        /// Provides access to change tracking information and operations for entity instances.
        /// </summary>
        ChangeTracker ChangeTracker { get; }

        /// <summary>
        /// Adds the given entity to the context.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The EntityEntry for the added entity.</returns>
        EntityEntry Add(object entity);

        /// <summary>
        /// Adds the given entity of type <typeparamref name="TEntity"/> to the context.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to add.</param>
        /// <returns>The EntityEntry for the added entity.</returns>
        EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Asynchronously adds the given entity to the context.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A ValueTask representing the asynchronous operation, with the EntityEntry for the added entity.</returns>
        ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously adds the given entity of type <typeparamref name="TEntity"/> to the context.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to add.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A ValueTask representing the asynchronous operation, with the EntityEntry for the added entity.</returns>
        ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;

        /// <summary>
        /// Adds the given entities to the context.
        /// </summary>
        /// <param name="entities">The entities to add.</param>
        void AddRange(IEnumerable<object> entities);

        /// <summary>
        /// Adds the given entities to the context.
        /// </summary>
        /// <param name="entities">The entities to add.</param>
        void AddRange(params object[] entities);

        /// <summary>
        /// Asynchronously adds the given entities to the context.
        /// </summary>
        /// <param name="entities">The entities to add.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously adds the given entities to the context.
        /// </summary>
        /// <param name="entities">The entities to add.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        Task AddRangeAsync(params object[] entities);

        /// <summary>
        /// Attaches the given entity to the context.
        /// </summary>
        /// <param name="entity">The entity to attach.</param>
        /// <returns>The EntityEntry for the attached entity.</returns>
        EntityEntry Attach(object entity);

        /// <summary>
        /// Attaches the given entity of type <typeparamref name="TEntity"/> to the context.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to attach.</param>
        /// <returns>The EntityEntry for the attached entity.</returns>
        EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Attaches the given entities to the context.
        /// </summary>
        /// <param name="entities">The entities to attach.</param>
        void AttachRange(params object[] entities);

        /// <summary>
        /// Attaches the given entities to the context.
        /// </summary>
        /// <param name="entities">The entities to attach.</param>
        void AttachRange(IEnumerable<object> entities);

        /// <summary>
        /// Finds an entity with the given primary key values.
        /// </summary>
        /// <param name="entityType">The type of the entity.</param>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>The found entity, or null.</returns>
        object? Find(Type entityType, params object[] keyValues);

        /// <summary>
        /// Finds an entity of type <typeparamref name="TEntity"/> with the given primary key values.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>The found entity, or null.</returns>
        TEntity? Find<TEntity>(params object[] keyValues) where TEntity : class;

        /// <summary>
        /// Asynchronously finds an entity with the given primary key values.
        /// </summary>
        /// <param name="entityType">The type of the entity.</param>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A ValueTask representing the asynchronous operation, with the found entity or null.</returns>
        ValueTask<object?> FindAsync(Type entityType, object[] keyValues, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously finds an entity with the given primary key values.
        /// </summary>
        /// <param name="entityType">The type of the entity.</param>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>A ValueTask representing the asynchronous operation, with the found entity or null.</returns>
        ValueTask<object?> FindAsync(Type entityType, params object[] keyValues);

        /// <summary>
        /// Asynchronously finds an entity of type <typeparamref name="TEntity"/> with the given primary key values.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <returns>A ValueTask representing the asynchronous operation, with the found entity or null.</returns>
        ValueTask<TEntity?> FindAsync<TEntity>(params object[] keyValues) where TEntity : class;

        /// <summary>
        /// Asynchronously finds an entity of type <typeparamref name="TEntity"/> with the given primary key values.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A ValueTask representing the asynchronous operation, with the found entity or null.</returns>
        ValueTask<TEntity?> FindAsync<TEntity>(object[] keyValues, CancellationToken cancellationToken) where TEntity : class;

        /// <summary>
        /// Removes the given entity from the context.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        /// <returns>The EntityEntry for the removed entity.</returns>
        EntityEntry Remove(object entity);

        /// <summary>
        /// Removes the given entity of type <typeparamref name="TEntity"/> from the context.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to remove.</param>
        /// <returns>The EntityEntry for the removed entity.</returns>
        EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Removes the given entities from the context.
        /// </summary>
        /// <param name="entities">The entities to remove.</param>
        void RemoveRange(params object[] entities);

        /// <summary>
        /// Removes the given entities from the context.
        /// </summary>
        /// <param name="entities">The entities to remove.</param>
        void RemoveRange(IEnumerable<object> entities);

        /// <summary>
        /// Updates the given entity in the context.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>The EntityEntry for the updated entity.</returns>
        EntityEntry Update(object entity);

        /// <summary>
        /// Updates the given entity of type <typeparamref name="TEntity"/> in the context.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to update.</param>
        /// <returns>The EntityEntry for the updated entity.</returns>
        EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Updates the given entities in the context.
        /// </summary>
        /// <param name="entities">The entities to update.</param>
        void UpdateRange(params object[] entities);

        /// <summary>
        /// Updates the given entities in the context.
        /// </summary>
        /// <param name="entities">The entities to update.</param>
        void UpdateRange(IEnumerable<object> entities);

        /// <summary>
        /// Gets the EntityEntry for the given entity.
        /// </summary>
        /// <param name="entity">The entity to get the entry for.</param>
        /// <returns>The EntityEntry for the entity.</returns>
        EntityEntry Entry(object entity);

        /// <summary>
        /// Gets the EntityEntry for the given entity of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity to get the entry for.</param>
        /// <returns>The EntityEntry for the entity.</returns>
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Gets a DbSet for the given entity type.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>A set for the given entity type.</returns>
        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <returns>The number of state entries written to the database.</returns>
        int SaveChanges();

        /// <summary>
        /// Saves all changes made in this context to the database, with an option to accept all changes on success.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">Indicates whether to accept all changes on success.</param>
        /// <returns>The number of state entries written to the database.</returns>
        int SaveChanges(bool acceptAllChangesOnSuccess);

        /// <summary>
        /// Asynchronously saves all changes made in this context to the database.
        /// </summary>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A Task representing the asynchronous operation, with the number of state entries written to the database.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously saves all changes made in this context to the database, with an option to accept all changes on success.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">Indicates whether to accept all changes on success.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A Task representing the asynchronous operation, with the number of state entries written to the database.</returns>
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        bool Equals(object obj);

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        int GetHashCode();

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        string? ToString();
    }
}
