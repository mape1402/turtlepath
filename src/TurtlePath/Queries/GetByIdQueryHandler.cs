namespace TurtlePath.Queries
{
    using TurtlePath.Models.Responses;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Domain.Identifier;
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Represents a query to retrieve a TurtlePath BaseEntity by its unique CId identifier.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public class GetByIdQuery<TEntity, TResponse> : GenericGetByIdQuery<TEntity, TResponse, CId>
        where TEntity : BaseEntity
        where TResponse : BaseResponse
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="id">The unique identifier used to retrieve the specific entity.</param>
        public GetByIdQuery(CId id) : base(id)
        {
        }
    }

    /// <summary>
    /// Provides a base implementation for handling queries that retrieve a TurtlePath BaseEntity by its CId identifier.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public abstract class GetByIdQueryHandler<TQuery, TEntity, TResponse> : GenericGetByIdQueryHandler<TQuery, TEntity, TResponse, CId>
        where TQuery : GetByIdQuery<TEntity, TResponse>
        where TEntity : BaseEntity
        where TResponse : BaseResponse
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected GetByIdQueryHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
