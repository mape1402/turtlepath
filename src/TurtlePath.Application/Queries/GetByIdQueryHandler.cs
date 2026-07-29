namespace TurtlePath.Application.Queries
{
    using TurtlePath.Application.Models.Responses;
    using TurtlePath.Contracts;
    using TurtlePath.Identifier;
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Represents a query to retrieve an entity by its unique identifier.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public class GetByIdQuery<TEntity, TResponse> : GetOneQuery<CId, TEntity, TResponse>
        where TEntity : BaseEntity
        where TResponse : BaseResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetByIdQuery{TEntity, TResponse}"/> class with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier used to retrieve the specific entity.</param>
        public GetByIdQuery(CId id)
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
    public abstract class GetByIdQueryHandler<TQuery, TEntity, TResponse> : GetOneQueryHandler<TQuery, CId, TEntity, TResponse>
        where TQuery : GetByIdQuery<TEntity, TResponse>
        where TEntity : BaseEntity
        where TResponse : BaseResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetByIdQueryHandler{TQuery, TEntity, TResponse}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected GetByIdQueryHandler(IServiceProvider serviceProvider) : base(serviceProvider) { }

        /// <inheritdoc/>
        protected override Expression<Func<TEntity, bool>> GetFilterExpression(TQuery request)
            => e => e.Id == request.Value;
    }
}
