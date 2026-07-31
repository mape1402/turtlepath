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
    public class GetByIdQuery<TEntity, TResponse, TKey> : GetOneQuery<TKey, TEntity, TResponse, TKey>
        where TEntity : class, IEntity<TKey>
        where TResponse : class, IBaseResponse<TKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetByIdQuery{TEntity, TResponse}"/> class with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier used to retrieve the specific entity.</param>
        public GetByIdQuery(TKey id)
        {
            Value = id;
        }
    }

    /// <summary>
    /// Represents a query to retrieve a TurtlePath BaseEntity by its unique CId identifier.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public class GetByIdQuery<TEntity, TResponse> : GetByIdQuery<TEntity, TResponse, CId>
        where TEntity : BaseEntity
        where TResponse : BaseResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetByIdQuery{TEntity, TResponse}"/> class with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier used to retrieve the specific entity.</param>
        public GetByIdQuery(CId id) : base(id)
        {
        }
    }

    /// <summary>
    /// Provides a base implementation for handling queries that retrieve an entity by its unique identifier.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <typeparam name="TKey">The entity identifier type.</typeparam>
    public abstract class GetByIdQueryHandler<TQuery, TEntity, TResponse, TKey> : GetOneQueryHandler<TQuery, TKey, TEntity, TResponse, TKey>
        where TQuery : GetByIdQuery<TEntity, TResponse, TKey>
        where TEntity : class, IEntity<TKey>
        where TResponse : class, IBaseResponse<TKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetByIdQueryHandler{TQuery, TEntity, TResponse}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected GetByIdQueryHandler(IServiceProvider serviceProvider) : base(serviceProvider) { }

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

    /// <summary>
    /// Provides a base implementation for handling queries that retrieve a TurtlePath BaseEntity by its CId identifier.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public abstract class GetByIdQueryHandler<TQuery, TEntity, TResponse> : GetByIdQueryHandler<TQuery, TEntity, TResponse, CId>
        where TQuery : GetByIdQuery<TEntity, TResponse>
        where TEntity : BaseEntity
        where TResponse : BaseResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetByIdQueryHandler{TQuery, TEntity, TResponse}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected GetByIdQueryHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
