namespace TurtlePath.Queries
{
    using TurtlePath.Models.Responses;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Domain.Identifier;
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Represents a query to retrieve an entity by its unique identifier.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <typeparam name="TKey">The entity identifier type.</typeparam>
    public class EntityGetByIdQuery<TEntity, TResponse, TKey> : EntityGetOneQuery<TKey, TEntity, TResponse, TKey>
        where TEntity : class, IEntity<TKey>
        where TResponse : class, IBaseResponse<TKey>
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="id">The unique identifier used to retrieve the specific entity.</param>
        public EntityGetByIdQuery(TKey id)
        {
            Value = id;
        }
    }

    /// <summary>
    /// Provides a base implementation for handling queries that retrieve an entity by its unique identifier.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <typeparam name="TKey">The entity identifier type.</typeparam>
    public abstract class EntityGetByIdQueryHandler<TQuery, TEntity, TResponse, TKey> : EntityGetOneQueryHandler<TQuery, TKey, TEntity, TResponse, TKey>
        where TQuery : EntityGetByIdQuery<TEntity, TResponse, TKey>
        where TEntity : class, IEntity<TKey>
        where TResponse : class, IBaseResponse<TKey>
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected EntityGetByIdQueryHandler(IServiceProvider serviceProvider) : base(serviceProvider) { }

        /// <inheritdoc/>
        protected override Expression<Func<TEntity, bool>> GetFilterExpression(TQuery request)
        {
            var entity = Expression.Parameter(typeof(TEntity), "entity");
            var id = Expression.Property(entity, nameof(IEntity<TKey>.Id));
            var value = Expression.Constant(request.Value, typeof(TKey));
            var equals = Expression.Equal(id, value);

            return Expression.Lambda<Func<TEntity, bool>>(equals, entity);
        }
    }
}
